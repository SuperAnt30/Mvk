using MvkClient.Renderer.Chunk;
using MvkClient.Setitings;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System.Collections.Generic;


namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера блока
    /// </summary>
    public class BlockRender
    {
        /// <summary>
        /// Пометка, разрушается ли блок и его стадия
        /// -1 не разрушается, 0-9 разрушается
        /// </summary>
        public int DamagedBlocksValue { get; set; } = -1;

        /// <summary>
        /// Построение стороны блока
        /// </summary>
        public BlockSide blockUV = new BlockSide();

        /// <summary>
        /// Объект рендера чанков
        /// </summary>
        protected readonly ChunkRender chunk;
        private ChunkBase chunkCheck;
        /// <summary>
        /// Объект блока кэш
        /// </summary>
        public BlockBase block;
        public BlockState blockState;
        
        /// <summary>
        /// Объект блока для проверки
        /// </summary>
        private BlockBase blockCheck;
        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int posChunkX;
        /// <summary>
        /// Позиция блока в чанке 0..255
        /// </summary>
        public int posChunkY;
        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int posChunkY0;
        /// <summary>
        /// Позиция блока в чанке 0..15
        /// </summary>
        public int posChunkZ;
        /// <summary>
        /// кэш коробка
        /// </summary>
        private Box cBox;
        /// <summary>
        /// кэш сторона блока
        /// </summary>
        private Face cFace;
        /// <summary>
        /// кэш Направление
        /// </summary>
        private int cSideInt;

        /// <summary>
        /// Видим только лицевую сторону полигона
        /// </summary>
        private bool cullFace = true;
        /// <summary>
        /// Тень на углах
        /// </summary>
        private readonly bool ambientOcclusion = false;

        private readonly int[] resultSide = new int[] { -1, -1, -1, -1, -1, -1 };

        public int cbX, cbY, cbZ;

        private int xcn, ycn, zcn;
        private int xc, yc, zc;
        private int xb, yb, zb;
        private int index;
        private float u1, v2;

        private int id;
        private ChunkStorage storage;
        private bool isDraw;
        private int stateLight;

        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRender(ChunkRender chunkRender, int cbX, int cbY, int cbZ)
        {
            this.cbX = cbX;
            this.cbY = cbY;
            this.cbZ = cbZ;
            ambientOcclusion = Setting.SmoothLighting;
            chunk = chunkRender;
        }

        /// <summary>
        /// Получть Сетку блока с возможностью двух сторон
        /// </summary>
        /// <returns>сетка</returns>
        public void RenderMesh()
        {
            xc = chunk.Position.x;
            zc = chunk.Position.y;
            int rs;
            if (posChunkY + 1 >= ChunkBase.COUNT_HEIGHT_BLOCK) rs = resultSide[0] = 0x0F;
            else rs = resultSide[0] = GetBlockSideState(posChunkX, posChunkY + 1, posChunkZ, false);
            if (posChunkY - 1 < 0) rs += resultSide[1] = 0x0F;
            else rs += resultSide[1] = GetBlockSideState(posChunkX, posChunkY - 1, posChunkZ, false);

            yc = posChunkY >> 4;
            rs += resultSide[2] = GetBlockSideState(posChunkX + 1, posChunkY, posChunkZ, true);
            rs += resultSide[3] = GetBlockSideState(posChunkX - 1, posChunkY, posChunkZ, true);
            rs += resultSide[4] = GetBlockSideState(posChunkX, posChunkY, posChunkZ - 1, true);
            rs += resultSide[5] = GetBlockSideState(posChunkX, posChunkY, posChunkZ + 1, true);

            if (rs != -6)
            {
                stateLight = block.UseNeighborBrightness ? (blockState.lightBlock << 4 | blockState.lightSky & 0xF) : -1;
                if (block.BackSide)
                {
                    cullFace = false;
                    RenderMeshBlock();
                    cullFace = true;
                }
                RenderMeshBlock();
            }
        }

        /// <summary>
        /// Подготовить кэш blockSideCache, прорисовывается ли сторона и её яркость
        /// </summary>
        private int GetBlockSideState(int x, int y, int z, bool isNearbyChunk)
        {
            if (isNearbyChunk)
            {
                xcn = x >> 4;
                zcn = z >> 4;
                xc = chunk.Position.x;
                zc = chunk.Position.y;
                // Определяем рабочий чанк соседнего блока
                if (xcn == 0 && zcn == 0)
                {
                    storage = chunk.StorageArrays[yc];
                }
                else
                {
                    xc += xcn;
                    zc += zcn;
                    chunkCheck = chunk.Chunk(xcn, zcn);
                    if (chunkCheck == null) return 0x0F; // Только яркость неба макс
                    storage = chunkCheck.StorageArrays[yc];
                }
            }
            else
            {
                storage = chunk.StorageArrays[y >> 4];
            }
            
            if (!storage.sky) return 0x0F; // Только яркость неба макс
            
            xb = x & 15;
            yb = y & 15;
            zb = z & 15;
            int i = yb << 8 | zb << 4 | xb;

            if (storage.countData > 0) id = storage.data[i] & 0xFFF;
            
            blockCheck = Blocks.blocksInt[id];

            isDraw = id == 0 || blockCheck.AllSideForcibly;
            UpIsDraw();
            // Яркость берётся из данных блока
            return isDraw ? (storage.lightBlock[i] << 4 | storage.lightSky[i] & 0xF) : -1;
        }

        /// <summary>
        /// Обновить прорисовывается сторона или нет, по флагам блоков между ними
        /// </summary>
        private void UpIsDraw()
        {
            //isDraw = id == 0 || (blockCheck.AllSideForcibly && cullFace);

            if (isDraw && (blockCheck.Material == block.Material && !blockCheck.BlocksNotSame)
                || (!cullFace && block.Material == EnumMaterial.Water
                    && (blockCheck.Material == EnumMaterial.Glass || blockCheck.Material == EnumMaterial.Lava || blockCheck.Material == EnumMaterial.Oil))) isDraw = false;
        }

        private void RenderMeshBlock()
        {
            int idB = 0;
            int idF = 0;
            Box[] boxes = block.GetBoxes(blockState.Met());
            int countB = boxes.Length;
            int countF = 0;
            while (idB < countB)
            {
                cBox = boxes[idB];
                countF = cBox.Faces.Length;
                idF = 0;
                while (idF < countF)
                {
                    cFace = cBox.Faces[idF];
                    if (cFace.side == -1)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            cSideInt = i;
                            RenderMeshSide();
                        }
                    }
                    else
                    {
                        cSideInt = cFace.side;
                        RenderMeshSide();
                    }
                    idF++;
                }
                idB++;
            }
        }

        /// <summary>
        /// Получть Сетку стороны блока с проверкой соседнего блока и разрушения его
        /// </summary>
        private void RenderMeshSide()
        {
            if (!block.BlocksNotSame)
            {
                if (!cullFace)
                {
                    vec3i pos = EnumFacing.DirectionVec(cSideInt);
                    yc = (posChunkY + pos.y) >> 4;
                    if (yc < 0 || yc > ChunkBase.COUNT_HEIGHT15) stateLight = 0x0F;
                    else stateLight = GetBlockSideState(posChunkX + pos.x, posChunkY + pos.y, posChunkZ + pos.z, true);
                }
                else
                {
                    stateLight = resultSide[cSideInt];
                }
            }
            
            //if (block.LightValue > 0) stateLight = 0xFF;

            if (stateLight != -1)
            {
                RenderMeshFace((byte)stateLight);

                if (DamagedBlocksValue != -1)
                {
                    Face face = cFace;
                    cFace = new Face((Pole)cSideInt, 4032 + DamagedBlocksValue, true, cFace.color);
                    RenderMeshFace((byte)stateLight);
                    cFace = face;
                }
            }
        }

        /// <summary>
        /// Генерация сетки стороны коробки
        /// </summary>
        private void RenderMeshFace(byte light)
        {
            u1 = cFace.u1;
            v2 = cFace.v2;

            ColorsLights colorLight = GenColors(light);

            blockUV.colorsr = colorLight.colorr;
            blockUV.colorsg = colorLight.colorg;
            blockUV.colorsb = colorLight.colorb;
            blockUV.lights = colorLight.light;
            blockUV.v1x = cBox.From.x + posChunkX;
            blockUV.v1y = cBox.From.y + posChunkY0;
            blockUV.v1z = cBox.From.z + posChunkZ;
            blockUV.v2x = cBox.To.x + posChunkX;
            blockUV.v2y = cBox.To.y + posChunkY0;
            blockUV.v2z = cBox.To.z + posChunkZ;
            blockUV.u1x = u1 + cBox.UVFrom.x;
            blockUV.u1y = v2 + cBox.UVTo.y;
            blockUV.u2x = u1 + cBox.UVTo.x;
            blockUV.u2y = v2 + cBox.UVFrom.y;
            blockUV.animationFrame = cFace.animationFrame;
            blockUV.animationPause = cFace.animationPause;
            blockUV.yawUV = cBox.RotateYawUV;
            blockUV.cullFace = cullFace;

            if (cBox.Translate.x != 0 || cBox.Translate.y != 0 || cBox.Translate.z != 0)
            {
                blockUV.translateX = cBox.Translate.x;
                blockUV.translateY = cBox.Translate.y;
                blockUV.translateZ = cBox.Translate.z;
                blockUV.isTranslate = true;
            }
            else
            {
                blockUV.isTranslate = false;
            }
            if (cBox.RotateYaw != 0 || cBox.RotatePitch != 0)
            {
                blockUV.posCenterX = posChunkX + .5f;
                blockUV.posCenterY = posChunkY0 + .5f;
                blockUV.posCenterZ = posChunkZ + .5f;
                blockUV.yaw = cBox.RotateYaw;
                blockUV.pitch = cBox.RotatePitch;
                blockUV.isRotate = true;
            }
            else
            {
                blockUV.isRotate = false;
            }
            blockUV.SideRotate(cSideInt);
        }

        /// <summary>
        /// Сгенерировать цвета на каждый угол, если надо то AmbientOcclusion
        /// </summary>
        private ColorsLights GenColors(byte light)
        {
            vec3 color = cFace.isColor ? GetBiomeColor(cbX, cbZ) : new vec3(1f);
            float lightPole = block.NoSideDimming ? 0f : 1f - LightPole();

            if (ambientOcclusion && block.АmbientOcclusion)
            {
                AmbientOcclusionLights ambient = GetAmbientOcclusionLights();
                lightPole *= .5f;
                return new ColorsLights(
                    ambient.GetColor(0, color, lightPole), ambient.GetColor(1, color, lightPole),
                    ambient.GetColor(2, color, lightPole), ambient.GetColor(3, color, lightPole),
                    ambient.GetLight(0, light), ambient.GetLight(1, light), 
                    ambient.GetLight(2, light), ambient.GetLight(3, light));
            }

            color.x -= lightPole; if (color.x < 0) color.x = 0;
            color.y -= lightPole; if (color.y < 0) color.y = 0;
            color.z -= lightPole; if (color.z < 0) color.z = 0;
            return new ColorsLights(color, light);
        }

        /// <summary>
        /// Получить цвет в зависимости от биома, цвет определяем потипу
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posZ"></param>
        /// <returns></returns>
        private vec3 GetBiomeColor(int posX, int posZ /* тип блока, трава, вода, листва */)
        {
            // подготовка для теста плавности цвета
            if (cFace.isColor)
            {
                if (block.EBlock == EnumBlock.Turf || block.EBlock == EnumBlock.TallGrass)
                {
                    if (posX >> 4 == -1 && posZ >> 4 == -1)
                    {
                        return new vec3(.76f, .53f, .25f);
                    }
                    else
                    {
                        return new vec3(.56f, .73f, .35f);
                    }
                }
                return cFace.color;
            }
            return new vec3(1f);
        }

        /// <summary>
        /// В зависимости от биома подбираем плавный цвет
        /// </summary>
        /// <returns></returns>
        //private vec3[] BiomeColor(/* тип блока, трава, вода, листва */)
        //{
        //    vec3i p = Block.Position.Position;
        //    vec3 a, b, c, d, e, f, g, h;
        //    a = GetBiomeColor(p.x + 1, p.z + 0);
        //    b = GetBiomeColor(p.x + 0, p.z + 1);
        //    c = GetBiomeColor(p.x + -1, p.z + 0);
        //    d = GetBiomeColor(p.x + 0, p.z + -1);

        //    e = GetBiomeColor(p.x + -1, p.z + -1);
        //    f = GetBiomeColor(p.x + -1, p.z + 1);
        //    g = GetBiomeColor(p.x + 1, p.z + 1);
        //    h = GetBiomeColor(p.x + 1, p.z + -1);
        //    return new vec3[] { c + d + e, c + b + f, a + b + g, a + d + h };
        //}

        /// <summary>
        /// Получить все 4 вершины AmbientOcclusion и яркости от блока и неба
        /// </summary>
        private AmbientOcclusionLights GetAmbientOcclusionLights()
        {
            AmbientOcclusionLight a, b, c, d, e, f, g, h;
            switch (cSideInt)
            {
                case 0:
                    a = GetAmbientOcclusionLight(1, 1, 0);
                    b = GetAmbientOcclusionLight(0, 1, 1);
                    c = GetAmbientOcclusionLight(-1, 1, 0);
                    d = GetAmbientOcclusionLight(0, 1, -1);
                    e = GetAmbientOcclusionLight(-1, 1, -1);
                    f = GetAmbientOcclusionLight(-1, 1, 1);
                    g = GetAmbientOcclusionLight(1, 1, 1);
                    h = GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case 1:
                    c = GetAmbientOcclusionLight(1, -1, 0);
                    b = GetAmbientOcclusionLight(0, -1, 1);
                    a = GetAmbientOcclusionLight(-1, -1, 0);
                    d = GetAmbientOcclusionLight(0, -1, -1);
                    h = GetAmbientOcclusionLight(-1, -1, -1);
                    g = GetAmbientOcclusionLight(-1, -1, 1);
                    f = GetAmbientOcclusionLight(1, -1, 1);
                    e = GetAmbientOcclusionLight(1, -1, -1);
                    break;
                case 2:
                    b = GetAmbientOcclusionLight(1, 1, 0);
                    a = GetAmbientOcclusionLight(1, 0, 1);
                    d = GetAmbientOcclusionLight(1, -1, 0);
                    c = GetAmbientOcclusionLight(1, 0, -1);
                    e = GetAmbientOcclusionLight(1, -1, -1);
                    h = GetAmbientOcclusionLight(1, -1, 1);
                    g = GetAmbientOcclusionLight(1, 1, 1);
                    f = GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case 3:
                    b = GetAmbientOcclusionLight(-1, 1, 0);
                    c = GetAmbientOcclusionLight(-1, 0, 1);
                    d = GetAmbientOcclusionLight(-1, -1, 0);
                    a = GetAmbientOcclusionLight(-1, 0, -1);
                    h = GetAmbientOcclusionLight(-1, -1, -1);
                    e = GetAmbientOcclusionLight(-1, -1, 1);
                    f = GetAmbientOcclusionLight(-1, 1, 1);
                    g = GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                case 4:
                    b = GetAmbientOcclusionLight(0, 1, -1);
                    a = GetAmbientOcclusionLight(1, 0, -1);
                    d = GetAmbientOcclusionLight(0, -1, -1);
                    c = GetAmbientOcclusionLight(-1, 0, -1);
                    e = GetAmbientOcclusionLight(-1, -1, -1);
                    h = GetAmbientOcclusionLight(1, -1, -1);
                    g = GetAmbientOcclusionLight(1, 1, -1);
                    f = GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                case 5:
                    b = GetAmbientOcclusionLight(0, 1, 1);
                    c = GetAmbientOcclusionLight(1, 0, 1);
                    d = GetAmbientOcclusionLight(0, -1, 1);
                    a = GetAmbientOcclusionLight(-1, 0, 1);
                    h = GetAmbientOcclusionLight(-1, -1, 1);
                    e = GetAmbientOcclusionLight(1, -1, 1);
                    f = GetAmbientOcclusionLight(1, 1, 1);
                    g = GetAmbientOcclusionLight(-1, 1, 1);
                    break;
                default:
                    a = b = c = d = e = f = g = h = new AmbientOcclusionLight();
                    break;
            }
            return new AmbientOcclusionLights(new AmbientOcclusionLight[] { a, b, c, d, e, f, g, h });
        }

        /// <summary>
        /// Затемнение стороны от стороны блока
        /// </summary>
        private float LightPole()
        {
            switch (cSideInt)
            {
                case 0: return cullFace ? 1f : .8f;
                case 2: return 0.7f;
                case 3: return 0.7f;
                case 4: return 0.85f;
                case 5: return 0.85f;
            }
            return cullFace ? 0.6f : 1f;
        }

        /// <summary>
        /// Подготовить кэш blockSideCache, прорисовывается ли сторона и её яркость
        /// Получть данные (AmbientOcclusion и яркость) одно стороны для вершины
        /// </summary>
        private AmbientOcclusionLight GetAmbientOcclusionLight(int x, int y, int z)
        {
            int pX = posChunkX + x;
            int pY = posChunkY + y;
            int pZ = posChunkZ + z;
            AmbientOcclusionLight aoLight = new AmbientOcclusionLight();

            xcn = (pX >> 4);
            zcn = (pZ >> 4);
            xc = chunk.Position.x + xcn;
            zc = chunk.Position.y + zcn;
            xb = pX & 15;
            zb = pZ & 15;
            aoLight.color = GetBiomeColor(xc << 4 | xb, zc << 4 | zb);

            // проверка высоты
            if (pY < 0)
            {
                aoLight.lightSky = 0;
                return aoLight;
            }
            if (pY >= ChunkBase.COUNT_HEIGHT_BLOCK)
            {
                aoLight.lightSky = 15;
                aoLight.aol = 1;
                return aoLight;
            }
            yc = pY >> 4;
            // Определяем рабочий чанк соседнего блока
            chunkCheck = (xc == chunk.Position.x && zc == chunk.Position.y) ? chunk : chunk.Chunk(xcn, zcn);
            if (chunkCheck == null)
            {
                aoLight.lightSky = 15;
                aoLight.aol = 1;
                return aoLight;
            }
            storage = chunkCheck.StorageArrays[yc];
            if (!storage.sky || storage.countData == 0)
            {
                // Только яркость неба макс
                aoLight.lightSky = 15;
                aoLight.aol = 1;
                return aoLight;
            }
            yb = pY & 15;
            index = yb << 8 | zb << 4 | xb;
            id = storage.data[index] & 0xFFF;
            blockCheck = Blocks.blocksInt[id];
            aoLight.aoc = blockCheck.АmbientOcclusion ? 1 : 0;
            aoLight.aol = blockCheck.IsNotTransparent() ? 0 : 1;

            //isDraw = id == 0 || (blockCheck.AllSideForcibly && blockCheck.Material != EnumMaterial.Lava);
            isDraw = id == 0 || (blockCheck.AllSideForcibly && blockCheck.UseNeighborBrightness);
            
            UpIsDraw();

            if (isDraw)
            {
                // Яркость берётся из данных блока
                aoLight.lightBlock = storage.lightBlock[index];
                aoLight.lightSky = storage.lightSky[index];
            }
            return aoLight;
        }

        /// <summary>
        /// Структура для вершин цвета и освещения
        /// </summary>
        private struct ColorsLights
        {
            //private readonly vec3[] color;
            public byte[] colorr;
            public byte[] colorg;
            public byte[] colorb;
            public byte[] light;

            public ColorsLights(vec3 color, byte light)
            {
                byte c = (byte)(color.x * 255);
                colorr = new byte[] { c, c, c, c };
                c = (byte)(color.y * 255);
                colorg = new byte[] { c, c, c, c };
                c = (byte)(color.z * 255);
                colorb = new byte[] { c, c, c, c };
                this.light = new byte[] { light, light, light, light };
            }
            public ColorsLights(vec3 color1, vec3 color2, vec3 color3, vec3 color4,
                byte light1, byte light2, byte light3, byte light4)
            {
                colorr = new byte[] { (byte)(color1.x * 255), (byte)(color2.x * 255), (byte)(color3.x * 255), (byte)(color4.x * 255) };
                colorg = new byte[] { (byte)(color1.y * 255), (byte)(color2.y * 255), (byte)(color3.y * 255), (byte)(color4.y * 255) };
                colorb = new byte[] { (byte)(color1.z * 255), (byte)(color2.z * 255), (byte)(color3.z * 255), (byte)(color4.z * 255) };
                light = new byte[] { light1, light2, light3, light4 };
            }
        }
        /// <summary>
        /// Структура для 4-ёх вершин цвета и освещения
        /// </summary>
        private struct AmbientOcclusionLights
        {
            private readonly int[] lightBlock;
            private readonly int[] lightSky;
            private readonly vec3[] colors;
            private readonly int[] aols;
            private readonly int[] aocs;

            public AmbientOcclusionLights(AmbientOcclusionLight[] aos)
            {
                // a, b, c, d, e, f, g, h
                // 0, 1, 2, 3, 4, 5, 6, 7

                lightBlock = new int[] {
                    aos[2].lightBlock + aos[3].lightBlock + aos[4].lightBlock,
                    aos[1].lightBlock + aos[2].lightBlock + aos[5].lightBlock,
                    aos[0].lightBlock + aos[1].lightBlock + aos[6].lightBlock,
                    aos[0].lightBlock + aos[3].lightBlock + aos[7].lightBlock
                };
                lightSky = new int[] {
                    aos[2].lightSky + aos[3].lightSky + aos[4].lightSky,
                    aos[1].lightSky + aos[2].lightSky + aos[5].lightSky,
                    aos[0].lightSky + aos[1].lightSky + aos[6].lightSky,
                    aos[0].lightSky + aos[3].lightSky + aos[7].lightSky
                };
                colors = new vec3[]
                {
                    aos[2].color + aos[3].color + aos[4].color,
                    aos[1].color + aos[2].color + aos[5].color,
                    aos[0].color + aos[1].color + aos[6].color,
                    aos[0].color + aos[3].color + aos[7].color
                };
                aols = new int[]
                {
                    aos[2].aol + aos[3].aol + aos[4].aol,
                    aos[1].aol + aos[2].aol + aos[5].aol,
                    aos[0].aol + aos[1].aol + aos[6].aol,
                    aos[0].aol + aos[3].aol + aos[7].aol
                };
                aocs = new int[]
                {
                    aos[2].aoc + aos[3].aoc + aos[4].aoc,
                    aos[1].aoc + aos[2].aoc + aos[5].aoc,
                    aos[0].aoc + aos[1].aoc + aos[6].aoc,
                    aos[0].aoc + aos[3].aoc + aos[7].aoc
                };
            }
            
            public byte GetLight(int index, byte light)
            {
                byte lb = (byte)((light & 0xF0) >> 4);
                byte ls = (byte)(light & 0xF);
                int count = 1 + aols[index];
                lb = (byte)((lightBlock[index] + lb) / count);
                ls = (byte)((lightSky[index] + ls) / count);

                return (byte)(lb << 4 | ls);
            }
            public vec3 GetColor(int index, vec3 color, float lightPole)
            {
                vec3 c = (colors[index] + color) / 4f * (1f - aocs[index] * .2f);
                c.x -= lightPole; if (c.x < 0) c.x = 0;
                c.y -= lightPole; if (c.y < 0) c.y = 0;
                c.z -= lightPole; if (c.z < 0) c.z = 0;
                return c;
            }
        }
        /// <summary>
        /// Структура данных (AmbientOcclusion и яркости от блока и неба) одно стороны для вершины
        /// </summary>
        private struct AmbientOcclusionLight
        {
            public byte lightBlock;
            public byte lightSky;
            public vec3 color;
            public int aol;
            public int aoc;
        }
    }
}
