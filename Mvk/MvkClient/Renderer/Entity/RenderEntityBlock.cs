using MvkClient.Renderer.Block;
using MvkServer.Util;
using MvkServer.World.Block;

namespace MvkClient.Renderer.Entity
{
    public class RenderEntityBlock : RenderDL
    {
        /// <summary>
        /// Тип блока
        /// </summary>
        private readonly EnumBlock enumBlock;

        public RenderEntityBlock(EnumBlock enumBlock)
        {
            this.enumBlock = enumBlock;
        }

        protected override void DoRender()
        {
            BlockRender blockRender = new BlockRender(Blocks.GetBlockCache(enumBlock), new BlockPos());
            blockRender.RenderVBOtoDL();
        }
    }
}
