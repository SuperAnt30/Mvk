using MvkClient.Renderer.Chunk;
using MvkClient.Setitings;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using SharpGL;
using System;
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
        /// Буфер всех блоков чанка
        /// </summary>
        public List<byte> buffer;
        /// <summary>
        /// Буфер одного блока для альфы
        /// </summary>
        public List<byte> bufferAlpha;

        /// <summary>
        /// Нужна ли проверка боковых сторон
        /// </summary>
        public bool check = true;
        /// <summary>
        /// Объект рендера чанков
        /// </summary>
        private readonly ChunkRender chunk;
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
        /// Глобальная позиция блока
        /// </summary>
        public BlockPos blockPos;
        /// <summary>
        /// позиция блока в чанке
        /// </summary>
        private vec3i posChunk;
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
        private Pole cSide;
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

        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRender(ChunkRender chunkRender)
        {
            ambientOcclusion = Setting.SmoothLighting;
            chunk = chunkRender;
        }

        /// <summary>
        /// Создание блока генерации для GUI
        /// </summary>
        public BlockRender(BlockBase block, BlockPos blockPos)
        {
            this.block = block;
            this.blockPos = blockPos;
        }

        public void Set(BlockBase block, BlockPos blockPos)
        {
            this.block = block;
            this.blockPos = blockPos;
            posChunk = new vec3i(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);
        }

        /// <summary>
        /// Получть Сетку блока с возможностью двух сторон
        /// </summary>
        /// <returns>сетка</returns>
        public void RenderMesh()
        {
            posChunk = new vec3i(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);

            if (block.UseNeighborBrightness)
            {
                resultSide[0] = GetResultSide(posChunk.x, posChunk.y, posChunk.z);
            }
            else
            {
                resultSide[0] = GetResultSide(posChunk.x, posChunk.y + 1, posChunk.z);
                resultSide[1] = GetResultSide(posChunk.x, posChunk.y - 1, posChunk.z);
                resultSide[2] = GetResultSide(posChunk.x + 1, posChunk.y, posChunk.z);
                resultSide[3] = GetResultSide(posChunk.x - 1, posChunk.y, posChunk.z);
                resultSide[4] = GetResultSide(posChunk.x, posChunk.y, posChunk.z - 1);
                resultSide[5] = GetResultSide(posChunk.x, posChunk.y, posChunk.z + 1);
            }

            if (resultSide[0] != -1 || resultSide[1] != -1 || resultSide[2] != -1
                || resultSide[3] != -1 || resultSide[4] != -1 || resultSide[5] != -1)
            {
                if (block.RenderType.HasFlag(BlockBase.EnumRenderType.BackSide))
                {
                    cullFace = false;
                    RenderMeshBlock();
                    cullFace = true;
                }
                RenderMeshBlock();
            }
        }

        /// <summary>
        /// Получить сетку блока с одной стороны
        /// </summary>
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
                    cSide = cFace.GetSide();
                    if (cSide == Pole.All)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            cSide = (Pole)i;
                            cSideInt = block.UseNeighborBrightness ? 0 : i;
                            if (check) RenderMeshSideCheck();
                            else RenderMeshFace(255);
                        }
                    }
                    else
                    {
                        cSideInt = block.UseNeighborBrightness ? 0 : (int)cSide;
                        if (check) RenderMeshSideCheck();
                        else RenderMeshFace(255);
                    }
                    idF++;
                }
                idB++;
            }

            //foreach (Box box in block.Boxes)
            //{
            //    cBox = box;
            //    foreach (Face face in box.Faces)
            //    {
            //        cFace = face;
            //        if (face.GetSide() == Pole.All)
            //        {
            //            for (int i = 0; i < 6; i++)
            //            {
            //                cSide = (Pole)i;
            //                cSideInt = i;
            //                if (check) RenderMeshSideCheck(buffer, resultSide);
            //                else buffer.AddRange(RenderMeshFace(255));
            //            }
            //        }
            //        else
            //        {
            //            cSide = face.GetSide();
            //            cSideInt = (int)cSide;
            //            if (check) RenderMeshSideCheck(buffer, resultSide);
            //            else buffer.AddRange(RenderMeshFace(255));
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Получть Сетку стороны блока с проверкой соседнего блока и разрушения его
        /// </summary>
        private void RenderMeshSideCheck()
        {
            if (resultSide[cSideInt] != -1)
            {
                byte light = (byte)resultSide[cSideInt];
                RenderMeshFace(light);
                if (DamagedBlocksValue != -1)
                {
                    Face face = cFace;
                    cFace = new Face(cSide, 4032 + DamagedBlocksValue, true, cFace.GetColor());
                    RenderMeshFace(light);
                    cFace = face;
                }

            }
        }


        /// <summary>
        /// Получить смещение на текстуру блока
        /// </summary>
        /// <returns>x = u1, Y = v1, z = u2, w = v2</returns>
        private vec4 GetUV()
        {
            float u1 = (cFace.GetNumberTexture() % 64) * 0.015625f;// 0.015625f;
            float v2 = cFace.GetNumberTexture() / 64 * 0.015625f;

            return new vec4(
                u1 + cBox.UVFrom.x, v2 + cBox.UVTo.y,
                u1 + cBox.UVTo.x, v2 + cBox.UVFrom.y
                );
        }

        /// <summary>
        /// Генерация сетки стороны коробки
        /// </summary>
        private void RenderMeshFace(byte light)
        {
            vec4 uv = GetUV();

            

            // vec4 color = cFace.GetIsColor() ? new vec4(cFace.GetColor(), 1f) : new vec4(1f);
            // подготовка для теста плавности цвета
            //if (Block.EBlock == EnumBlock.Turf && cFace.GetIsColor() && Chunk.Position.x == -1 && Chunk.Position.y == -1)
            //{
            //    color = new vec4(.76f, .53f, .25f, 1f);
            //}

            //bool isWater = Block.EBlock == EnumBlock.Water;

            vec3 pos = new vec3(posChunk.x, posChunk.y & 15, posChunk.z);

            ColorsLights colorLight = GenColors(light);

            BlockSide blockUV = new BlockSide(
                colorLight.GetColor(),
                colorLight.GetLight(),
                pos + cBox.From,
                pos + cBox.To,
                new vec2(uv.x, uv.y),
                new vec2(uv.z, uv.w),
                cFace.AnimationFrame(),
                cFace.AnimationPause()
            );

            blockUV.SetYawUV(cBox.RotateYawUV);
            blockUV.Translate(cBox.Translate);
            if (cBox.RotateYaw != 0 || cBox.RotatePitch != 0)
            {
                blockUV.Rotate(pos + .5f, cBox.RotateYaw, cBox.RotatePitch);
            }

            //vec2 rot = block.GetRotateMetdata(blockState.Met());
            //if (rot.x != 0 || rot.y != 0)
            //{
            //    blockUV.Rotate(pos + .5f, rot.x, rot.y);
            //}

            blockUV.BufferByte.buffer = block.Translucent ? bufferAlpha : buffer;
            //if (block.IsAlpha) bufferAlpha.AddRange(blockUV.BufferByte.ToArray());
            //else buffer.AddRange(blockUV.BufferByte.ToArray());

            
            blockUV.Side(cSide, cullFace);

            //if (block.IsAlpha) bufferAlpha.AddRange(blockUV.BufferByte.ToArray());
            //else buffer.AddRange(blockUV.BufferByte.ToArray());
        }


        byte l;
        /// <summary>
        /// Сгенерировать цвета на каждый угол, если надо то AmbientOcclusion
        /// </summary>
        private ColorsLights GenColors(byte light)
        {
            l = light;
            vec3 color = cFace.GetIsColor() ? GetBiomeColor(blockPos.X, blockPos.Z) : new vec3(1f);
            float lightPole = block.RenderType.HasFlag(BlockBase.EnumRenderType.NoSideDimming) ? 0f : 1f - LightPole();

            color.x -= lightPole; if (color.x < 0) color.x = 0;
            color.y -= lightPole; if (color.y < 0) color.y = 0;
            color.z -= lightPole; if (color.z < 0) color.z = 0;

            if (ambientOcclusion && block.RenderType.HasFlag(BlockBase.EnumRenderType.АmbientOcclusion) && block.IsNotTransparent())
            {
                // AmbientOcclusion условие, что блок целый кубический, и не является альфой
                //vec3[] colorAO;
                AmbientOcclusionLights ambient = GetAmbientOcclusionLights();
                //vec4 ao = new vec4(1) - ambient.GetAO();
                /* if тип блока, трава, вода, листва */
                //if (Block.EBlock == EnumBlock.Turf && cSide == Pole.Up)
                //{
                //    vec3[] c = BiomeColor();
                //colorAO = new vec3[] { (color + c[0]) / 4f * ao.x, (color + c[1]) / 4f * ao.y,
                //    (color + c[2]) / 4f * ao.z, (color + c[3]) / 4f * ao.w };
                //}
                //else
                //{
                    // ao работает чисто от яркости света, тёмный блок уже даёт темноту!!!
                    //colorAO = new vec3[] { color * ao.x, color * ao.y, color * ao.z, color * ao.w };
                    //colorAO = new vec3[] { color, color, color, color };
                    //colorAO = new vec3[] { ambient.GetColor(0, color), ambient.GetColor(1, color),
                    //    ambient.GetColor(2, color), ambient.GetColor(3, color) };
                //}
                return new ColorsLights(
                    ambient.GetColor(0, color), ambient.GetColor(1, color),
                    ambient.GetColor(2, color), ambient.GetColor(3, color),
                    ambient.GetLight(0, light), ambient.GetLight(1, light), 
                    ambient.GetLight(2, light), ambient.GetLight(3, light));
            }
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
            if (cFace.GetIsColor())
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
                return cFace.GetColor();
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
            switch (cSide)
            {
                case Pole.Up:
                    a = GetAmbientOcclusionLight(1, 1, 0);
                    b = GetAmbientOcclusionLight(0, 1, 1);
                    c = GetAmbientOcclusionLight(-1, 1, 0);
                    d = GetAmbientOcclusionLight(0, 1, -1);
                    e = GetAmbientOcclusionLight(-1, 1, -1);
                    f = GetAmbientOcclusionLight(-1, 1, 1);
                    g = GetAmbientOcclusionLight(1, 1, 1);
                    h = GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case Pole.Down:
                    c = GetAmbientOcclusionLight(1, -1, 0);
                    b = GetAmbientOcclusionLight(0, -1, 1);
                    a = GetAmbientOcclusionLight(-1, -1, 0);
                    d = GetAmbientOcclusionLight(0, -1, -1);
                    h = GetAmbientOcclusionLight(-1, -1, -1);
                    g = GetAmbientOcclusionLight(-1, -1, 1);
                    f = GetAmbientOcclusionLight(1, -1, 1);
                    e = GetAmbientOcclusionLight(1, -1, -1);
                    break;
                case Pole.East:
                    b = GetAmbientOcclusionLight(1, 1, 0);
                    a = GetAmbientOcclusionLight(1, 0, 1);
                    d = GetAmbientOcclusionLight(1, -1, 0);
                    c = GetAmbientOcclusionLight(1, 0, -1);
                    e = GetAmbientOcclusionLight(1, -1, -1);
                    h = GetAmbientOcclusionLight(1, -1, 1);
                    g = GetAmbientOcclusionLight(1, 1, 1);
                    f = GetAmbientOcclusionLight(1, 1, -1);
                    break;
                case Pole.West:
                    b = GetAmbientOcclusionLight(-1, 1, 0);
                    c = GetAmbientOcclusionLight(-1, 0, 1);
                    d = GetAmbientOcclusionLight(-1, -1, 0);
                    a = GetAmbientOcclusionLight(-1, 0, -1);
                    h = GetAmbientOcclusionLight(-1, -1, -1);
                    e = GetAmbientOcclusionLight(-1, -1, 1);
                    f = GetAmbientOcclusionLight(-1, 1, 1);
                    g = GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                case Pole.South:
                    b = GetAmbientOcclusionLight(0, 1, 1);
                    c = GetAmbientOcclusionLight(1, 0, 1);
                    d = GetAmbientOcclusionLight(0, -1, 1);
                    a = GetAmbientOcclusionLight(-1, 0, 1);
                    h = GetAmbientOcclusionLight(-1, -1, 1);
                    e = GetAmbientOcclusionLight(1, -1, 1);
                    f = GetAmbientOcclusionLight(1, 1, 1);
                    g = GetAmbientOcclusionLight(-1, 1, 1);
                    break;
                case Pole.North:
                    b = GetAmbientOcclusionLight(0, 1, -1);
                    a = GetAmbientOcclusionLight(1, 0, -1);
                    d = GetAmbientOcclusionLight(0, -1, -1);
                    c = GetAmbientOcclusionLight(-1, 0, -1);
                    e = GetAmbientOcclusionLight(-1, -1, -1);
                    h = GetAmbientOcclusionLight(1, -1, -1);
                    g = GetAmbientOcclusionLight(1, 1, -1);
                    f = GetAmbientOcclusionLight(-1, 1, -1);
                    break;
                default:
                    a = b = c = d = e = f = g = h = new AmbientOcclusionLight(255, new vec3(1));
                    break;
            }
            return new AmbientOcclusionLights(new AmbientOcclusionLight[] { a, b, c, d, e, f, g, h });
           // return new vec4(c + d + e, c + b + f, a + b + g, a + d + h);
        }

        /// <summary>
        /// Получть данные (AmbientOcclusion и яркость) одно стороны для вершины
        /// </summary>
        private AmbientOcclusionLight GetAmbientOcclusionLight(int x, int y, int z)
        {
            ResultSide resultSide = GetResultSideAO(true, posChunk.x + x, posChunk.y + y, posChunk.z + z);
            // Параметр затемнение одного угла к блоку
            //float color = (!resultSide.IsEmpty() && resultSide.BlockCache().IsNotTransparent() && resultSide.BlockCache().IsFullCube)
            //    ? .1f : 0;
            return new AmbientOcclusionLight(resultSide.Light(), resultSide.GetColor());
        }

        /// <summary>
        /// Затемнение стороны от стороны блока
        /// </summary>
        private float LightPole()
        {
            switch (cSide)
            {
                case Pole.Up: return cullFace ? 1f : .8f;
                case Pole.South: return 0.85f;
                case Pole.East: return 0.7f;
                case Pole.West: return 0.7f;
                case Pole.North: return 0.85f;
            }
            return cullFace ? 0.6f : 1f;
        }

        /// <summary>
        /// Рендер блока VBO, конвертация из  VBO в DisplayList
        /// </summary>
        public void RenderVBOtoDL()
        {
            if (block.Translucent) bufferAlpha = new List<byte>();
            else buffer = new List<byte>();
            check = false;
            RenderMeshBlock();
            byte[] buffer2 = block.Translucent ? bufferAlpha.ToArray() : buffer.ToArray();

            GLRender.PushMatrix();
            {
                GLRender.Begin(OpenGL.GL_TRIANGLES);
                for (int i = 0; i < buffer2.Length; i += 28)
                {
                    float r = buffer2[i + 20] / 255f;
                    float g = buffer2[i + 21] / 255f;
                    float b = buffer2[i + 22] / 255f;
                    GLRender.Color(r, g, b);
                    float u = BitConverter.ToSingle(buffer2, i + 12);
                    float v = BitConverter.ToSingle(buffer2, i + 16);
                    GLRender.TexCoord(u, v);
                    float x = BitConverter.ToSingle(buffer2, i);
                    float y = BitConverter.ToSingle(buffer2, i + 4);
                    float z = BitConverter.ToSingle(buffer2, i + 8);
                    GLRender.Vertex(x - .5f, y - .5f, z - .5f);
                }
                GLRender.End();
            }
            GLRender.PopMatrix();
        }

        /// <summary>
        /// Получить яркость (блока + неба) стороны с соседним блоком
        /// </summary>
        private int GetResultSide(int x, int y, int z)
        {
            if (!check)
            {
                // Яркость неба и блока Макс
                return 0xFF;
            }

            int yc = y >> 4;
            // проверка высоты
            if (yc < 0 || yc >= ChunkBase.COUNT_HEIGHT)
            {
                // Только яркость неба макс
                return 0x0F;
            }

            int xc = chunk.Position.x + (x >> 4);
            int zc = chunk.Position.y + (z >> 4);
            int xv = x & 15;
            int yv = y & 15;
            int zv = z & 15;

            // Определяем рабочий чанк соседнего блока
            chunkCheck = (xc == chunk.Position.x && zc == chunk.Position.y) ? chunk : chunk.Chunk(new vec2i(xc, zc));

            if (chunkCheck == null || !chunkCheck.StorageArrays[yc].IsSky())
            {
                // Только яркость неба макс
                return 0x0F;
            }

            int id = 0;
            if (!chunkCheck.StorageArrays[yc].IsEmptyData())
            {
                id = chunkCheck.StorageArrays[yc].GetData(xv, yv, zv) & 0xFFF;
            }
            blockCheck = Blocks.BlocksInt[id];

            // TODO:: material IsTransparent
            bool isDraw = id == 0 || !blockCheck.IsNotTransparent();

            // Для слияния однотипных блоков
            //if (isDraw && id == blockState.Id())
            if (isDraw && block.Translucent && (blockCheck.EBlock == block.EBlock || blockCheck.Material == block.Material))
            //|| (Block.Material == EnumMaterial.Water && block.IsAlpha)))
            // TODO::2022-04-20 нужен новый слой на воде между альфой, он просто цвет
            {
                isDraw = false;
            }
            if (!isDraw && block.RenderType.HasFlag(BlockBase.EnumRenderType.AllSide))
            {
                isDraw = true;
            }
            if (isDraw)
            {
                //return 136;
                // Яркость берётся из данных блока
                return chunkCheck.StorageArrays[yc].GetLightsFor(xv, yv, zv);
            }
            // Блок без яркости, к примеру сплошной блок
            return -1;
        }

        /// <summary>
        /// Получить результат стороны с соседним блоком для AmbientOcclusion
        /// </summary>
        private ResultSide GetResultSideAO(bool check, int x, int y, int z)
        {
            if (!check) return new ResultSide(true);

            //vec3i pos = posChunk + EnumFacing.DirectionVec(side);
            int xc = this.chunk.Position.x + (x >> 4);
            int yc = y >> 4;
            int zc = this.chunk.Position.y + (z >> 4);
            int xv = x & 15;
            int yv = y & 15;
            int zv = z & 15;
            vec3 colorBiome = GetBiomeColor(xc << 4 | xv, zc << 4 | zv);

            // проверка высоты
            if (yc < 0 || yc >= ChunkBase.COUNT_HEIGHT) return new ResultSide(0x0F, new vec3(1f));

            // Определяем рабочий чанк соседнего блока
            ChunkBase chunk = (xc == this.chunk.Position.x && zc == this.chunk.Position.y) ? this.chunk
                : this.chunk.Chunk(new vec2i(xc, zc));

            if (chunk == null || !chunk.StorageArrays[yc].IsSky())
            {
                return new ResultSide(0x0F, colorBiome); // 1f
            }

            int id = 0;
            if (!chunk.StorageArrays[yc].IsEmptyData())
            {
                id = chunk.StorageArrays[yc].GetData(xv, yv, zv) & 0xFFF;
            }
            blockCheck = Blocks.BlocksInt[id];

            //BlockBase block = chunk.GetBlockState(xv, y, zv).GetBlock();

            // TODO:: material IsTransparent
            bool isDraw = id == 0 || !blockCheck.IsNotTransparent();
            //bool isDraw = block.IsAir || !block.IsNotTransparent();

            // Для слияния однотипных блоков
            if (isDraw && (blockCheck.EBlock == block.EBlock || blockCheck.Material == block.Material)) 
                //|| (Block.Material == EnumMaterial.Water && block.IsAlpha)))
                // TODO::2022-04-20 нужен новый слой на воде между альфой, он просто цвет
            {
                isDraw = false;
            }

            if (!isDraw && block.RenderType.HasFlag(BlockBase.EnumRenderType.AllSide)) isDraw = true;

            byte light = chunk.StorageArrays[yc].GetLightsFor(xv, yv, zv);

            if (id == 9) light = l;

            return new ResultSide(
                isDraw,
                light,//(byte)(id == 0 ? light : 0x00),//light,
                blockCheck,
                colorBiome
            );
        }

        /// <summary>
        /// Результат стороны блока
        /// </summary>
        private struct ResultSide
        {
            private readonly bool isDraw;
            private readonly byte light;
            private readonly BlockBase block;
            private readonly bool body;
            private readonly vec3 color;

            /// <summary>
            /// Результат стороны блока
            /// </summary>
            /// <param name="isDraw">Прорисовывать ли сторону</param>
            /// <param name="light">Общаяя яркость света неба и блока в одном байте</param>
            /// <param name="blockCache">блок кэша для параметров</param>
            public ResultSide(bool isDraw, byte light, BlockBase blockCache, vec3 color)
            {
                this.isDraw = isDraw;
                this.light = light;
                this.color = color;
                block = blockCache;
                body = true;
            }

            public ResultSide(byte light, vec3 color)
            {
                // TODO::2022-04-19 надо как-то пометить, чтоб не смешивал этот блок
                isDraw = false;
                this.light = light;
                this.color = color;
                block = null;
                body = false;
            }

            public ResultSide(bool isDraw)
            {
                this.isDraw = isDraw;
                light = 255;
                color = new vec3(1);
                block = null;
                body = true;
            }

            /// <summary>
            /// Прорисовывать ли сторону
            /// </summary>
            public bool IsDraw() => isDraw;
            /// <summary>
            /// Общаяя яркость света неба и блока в одном байте
            /// </summary>
            public byte Light() => light;
            /// <summary>
            /// Блок кэша
            /// </summary>
            public BlockBase BlockCache() => block;
            /// <summary>
            /// Вернуть цвет
            /// </summary>
            public vec3 GetColor() => color;
            /// <summary>
            /// Пустой ли объект
            /// </summary>
            public bool IsEmpty() => !body;
        }
        /// <summary>
        /// Структура для вершин цвета и освещения
        /// </summary>
        private struct ColorsLights
        {
            private readonly vec3[] color;
            private readonly byte[] light;

            public ColorsLights(vec3 color, byte light)
            {
                this.color = new vec3[] { color, color, color, color };
                this.light = new byte[] { light, light, light, light };
            }
            public ColorsLights(vec3 color1, vec3 color2, vec3 color3, vec3 color4,
                byte light1, byte light2, byte light3, byte light4)
            {
                color = new vec3[] { color1, color2, color3, color4 };
                light = new byte[] { light1, light2, light3, light4 };
            }

            public vec3[] GetColor() => color;
            public byte[] GetLight() => light;
        }
        /// <summary>
        /// Структура для 4-ёх вершин цвета и освещения
        /// </summary>
        private struct AmbientOcclusionLights
        {
            //private readonly float[] ao;
            private readonly int[] lightBlock;
            private readonly int[] lightSky;
            private readonly vec3[] colors;

            public AmbientOcclusionLights(AmbientOcclusionLight[] aos)
            {
                // a, b, c, d, e, f, g, h
                // 0, 1, 2, 3, 4, 5, 6, 7

                //ao = new float[] {
                //    aos[2].GetAO() + aos[3].GetAO() + aos[4].GetAO(),
                //    aos[1].GetAO() + aos[2].GetAO() + aos[5].GetAO(),
                //    aos[0].GetAO() + aos[1].GetAO() + aos[6].GetAO(),
                //    aos[0].GetAO() + aos[3].GetAO() + aos[7].GetAO()
                //};
                lightBlock = new int[] {
                    aos[2].GetLBlock() + aos[3].GetLBlock() + aos[4].GetLBlock(),
                    aos[1].GetLBlock() + aos[2].GetLBlock() + aos[5].GetLBlock(),
                    aos[0].GetLBlock() + aos[1].GetLBlock() + aos[6].GetLBlock(),
                    aos[0].GetLBlock() + aos[3].GetLBlock() + aos[7].GetLBlock()
                };
                lightSky = new int[] {
                    aos[2].GetLSky() + aos[3].GetLSky() + aos[4].GetLSky(),
                    aos[1].GetLSky() + aos[2].GetLSky() + aos[5].GetLSky(),
                    aos[0].GetLSky() + aos[1].GetLSky() + aos[6].GetLSky(),
                    aos[0].GetLSky() + aos[3].GetLSky() + aos[7].GetLSky()
                };
                colors = new vec3[]
                {
                    aos[2].GetColor() + aos[3].GetColor() + aos[4].GetColor(),
                    aos[1].GetColor() + aos[2].GetColor() + aos[5].GetColor(),
                    aos[0].GetColor() + aos[1].GetColor() + aos[6].GetColor(),
                    aos[0].GetColor() + aos[3].GetColor() + aos[7].GetColor()
                };
            }
            
            //public vec4 GetAO() => new vec4(ao[0], ao[1], ao[2], ao[3]);
            public byte GetLight(int index, byte light)
            {
                byte lb = (byte)((light & 0xF0) >> 4);
                byte ls = (byte)(light & 0xF);

                //byte lb = ChunkStorage.GetLightFor(light, EnumSkyBlock.Block);
                //byte ls = ChunkStorage.GetLightFor(light, EnumSkyBlock.Sky);

                lb = (byte)((lightBlock[index] + lb) / 4);
                ls = (byte)((lightSky[index] + ls) / 4);

                return (byte)(lb << 4 | ls);
            }
            public vec3 GetColor(int index, vec3 color) => (colors[index] + color) / 4f;
        }
        /// <summary>
        /// Структура данных (AmbientOcclusion и яркости от блока и неба) одно стороны для вершины
        /// </summary>
        private struct AmbientOcclusionLight
        {
            //private readonly float ao;
            private readonly byte lightBlock;
            private readonly byte lightSky;
            private readonly vec3 color;

            public AmbientOcclusionLight(/*float ao, */byte light, vec3 color)
            {
               // this.ao = ao;
                lightBlock = (byte)((light & 0xF0) >> 4);
                lightSky = (byte)(light & 0xF);
                this.color = color;
            }

           // public float GetAO() => ao;
            public byte GetLBlock() => lightBlock;
            public byte GetLSky() => lightSky;
            public vec3 GetColor() => color;
        }
    }
}
