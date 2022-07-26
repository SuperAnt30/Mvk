using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Отладочный блок
    /// </summary>
    public class BlockDebug : BlockBase
    {
        /// <summary>
        /// Отладочный блок
        /// </summary>
        public BlockDebug()
        {
            //Boxes = new Box[] { new Box(1) };

            Particle = 1;
            FullBlock = false;
            АmbientOcclusion = false;
            Shadow = false;
            AllSideForcibly = true;
            BlocksNotSame = true;
            UseNeighborBrightness = true;
           // IsCollidable = false;
            IsReplaceable = true;
            LightOpacity = 0;
            //LightValue = 15;
            Material = EnumMaterial.Stone;
            InitBoxs();
        }

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos blockPos, int met)
        {
            vec3 pos = blockPos.ToVec3();
            vec3 min, max;

            //AxisAlignedBB[] aabbs = new AxisAlignedBB[1];
            //min = new vec3(0);
            //max = new vec3(1, MvkStatic.Xy[8], 1);
            //aabbs[0] = new AxisAlignedBB(pos + min, pos + max);


            AxisAlignedBB[] aabbs = new AxisAlignedBB[2];
            min = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]);
            max = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]);
            aabbs[0] = new AxisAlignedBB(pos + min, pos + max);
            min = new vec3(MvkStatic.Xy[1], MvkStatic.Xy[8], MvkStatic.Xy[1]);
            max = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]);
            aabbs[1] = new AxisAlignedBB(pos + min, pos + max);

            //AxisAlignedBB[] aabbs = new AxisAlignedBB[2];
            //min = new vec3(0);
            //max = new vec3(MvkStatic.Xy[16], MvkStatic.Xy[8], MvkStatic.Xy[16]);
            //aabbs[0] = GetBoundingBox();
            //min = new vec3(0, MvkStatic.Xy[9], 0);
            //max = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[16]);
            //aabbs[1] = GetBoundingBox();
            return aabbs;
        }

        /// <summary>
        /// Получить вращение блока за счёт етданных
        /// </summary>
        //public override vec2 GetRotateMetdata(int met)
        //{
        //    return new vec2(glm.pi90, 0);
        //}

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met)
        {
            if (met == 0) return boxes[0];

            //=> boxes[0];
            return boxes[2];
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            boxes = new Box[4][];

            //boxes[0] = new Box[] {
            //    new Box()
            //    {
            //        From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
            //        To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]),
            //        UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[1]),
            //        UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[15]),
            //        Faces = new Face[] { new Face(Pole.Down, 1) }
            //    },
            //    new Box()
            //    {
            //        From = new vec3(MvkStatic.Xy[8], 0, MvkStatic.Xy[1]),
            //        To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]),
            //        UVFrom = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[1]),
            //        UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[15]),
            //        Faces = new Face[] { new Face(Pole.Up, 1) }
            //    },
            //    new Box()
            //    {
            //        From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
            //        To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]),
            //        UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[1]),
            //        UVTo = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[15]),
            //        Faces = new Face[] { new Face(Pole.Up, 1) }
            //    },
            //    new Box()
            //    {
            //        From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
            //        To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]),
            //        UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[8]),
            //        UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[16]),
            //        Faces = new Face[]
            //        {
            //            new Face(Pole.North, 1),
            //            new Face(Pole.South, 1),
            //            new Face(Pole.East, 1)
            //        }
            //    },
            //    new Box()
            //    {
            //        From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
            //        To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[16], MvkStatic.Xy[15]),
            //        UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[0]),
            //        UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[16]),
            //        Faces = new Face[] { new Face(Pole.West, 1) }
            //    },
            //    new Box()
            //    {
            //        From = new vec3(MvkStatic.Xy[1], MvkStatic.Xy[8], MvkStatic.Xy[1]),
            //        To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]),
            //        UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[0]),
            //        UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[8]),
            //        Faces = new Face[] { new Face(Pole.East, 1) }
            //    },
            //    new Box()
            //    {
            //        From = new vec3(MvkStatic.Xy[1], MvkStatic.Xy[8], MvkStatic.Xy[1]),
            //        To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]),
            //        UVFrom = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[0]),
            //        UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[8]),
            //        Faces = new Face[]
            //        {
            //            new Face(Pole.North, 1),
            //            new Face(Pole.South, 1)
            //        }
            //    }
            //};

            boxes[0] = new Box[] {
                //new Box()
                //{
                //    From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
                //    To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]),
                //    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[1]),
                //    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[15]),
                //    Faces = new Face[] { new Face(Pole.Down, 1) }
                //},
                //new Box()
                //{
                //    From = new vec3(MvkStatic.Xy[8], 0, MvkStatic.Xy[1]),
                //    To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]),
                //    UVFrom = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[1]),
                //    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[15]),
                //    Faces = new Face[] { new Face(Pole.Up, 1) }
                //},
                //new Box()
                //{
                //    From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
                //    To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]),
                //    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[1]),
                //    UVTo = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[15]),
                //    Faces = new Face[] { new Face(Pole.Up, 1) }
                //},
                //new Box()
                //{
                //    From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
                //    To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]),
                //    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[8]),
                //    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[16]),
                //    Faces = new Face[]
                //    {
                //        new Face(Pole.North, 1),
                //        new Face(Pole.South, 1),
                //        new Face(Pole.East, 1),
                //        new Face(Pole.Down, 1),
                //         new Face(Pole.Up, 1)
                //    }
                //},
                //new Box()
                //{
                //    From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
                //    To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[16], MvkStatic.Xy[15]),
                //    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[0]),
                //    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[16]),
                //    Faces = new Face[] { new Face(Pole.West, 1) }
                //},
                //new Box()
                //{
                //    From = new vec3(MvkStatic.Xy[1], MvkStatic.Xy[8], MvkStatic.Xy[1]),
                //    To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]),
                //    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[0]),
                //    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[8]),
                //    Faces = new Face[] { new Face(Pole.East, 1) }
                //},
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[1], MvkStatic.Xy[8], MvkStatic.Xy[1]),
                    To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]),
                    UVFrom = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[0]),
                    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[8]),
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 1),
                        new Face(Pole.North, 1),
                        new Face(Pole.South, 1),
                        new Face(Pole.East, 1),
                        new Face(Pole.West, 1)
                    }
                }
            };

            boxes[2] = new Box[] {
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[1], MvkStatic.Xy[8], MvkStatic.Xy[8]),
                    To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[16], MvkStatic.Xy[15]),
                    UVFrom = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[0]),
                    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[8]),
                    Faces = new Face[]
                        {
                            new Face(Pole.Up, 1),
                            new Face(Pole.North, 1),
                            new Face(Pole.South, 1),
                            new Face(Pole.East, 1),
                            new Face(Pole.West, 1)
                        }
                }
            };

            boxes[1] = new Box[boxes[0].Length];

            int i = 0;

            //foreach (Box box in boxes[0])
            //{
            //    Face[] faces = new Face[box.Faces.Length];
            //    for (int f = 0; f < box.Faces.Length; f++)
            //    {
            //        faces[f] = new Face(Rotate(box.Faces[f].GetSide()), box.Faces[f].GetNumberTexture());
            //    }
            //    boxes[1][i] = new Box()
            //    {
            //        From = Rotate(box.From),
            //        To = Rotate(box.To),
            //        UVFrom = box.UVFrom,
            //        UVTo = box.UVTo,
            //        Faces = faces
            //    };
            //    i++;
            //}
        }

        //private vec3 Rotate(vec3 pos)
        //{
        //    pos -= new vec3(.5f);
        //    pos = glm.rotate(pos, -glm.pi90, new vec3(0, 1f, 0));
        //    return pos + new vec3(.5f);
        //}

        //private Pole Rotate(Pole pole)
        //{
        //    switch(pole)
        //    {
        //        case Pole.East: return Pole.North;
        //        case Pole.North: return Pole.West;
        //        case Pole.West: return Pole.South;
        //        case Pole.South: return Pole.East;



        //            //case Pole.East: return Pole.South;
        //            //case Pole.South: return Pole.West;
        //            //case Pole.West: return Pole.North;
        //            //case Pole.North: return Pole.East;
        //    }
        //    return pole;
        //}

        /// <summary>
        /// Установить блок
        /// </summary>
        /// <param name="side">Сторона на какой ставим блок</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool Put(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            return base.Put(worldIn, blockPos,
                new BlockState(state.Id(), side == Pole.Up ? 1 : 0, state.lightBlock, state.lightSky),
                side, facing);
        }

    }
}
