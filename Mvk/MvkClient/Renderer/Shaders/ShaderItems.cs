using SharpGL;
using System.Collections.Generic;

namespace MvkClient.Renderer.Shaders
{
    /// <summary>
    /// Колекция шейдеров
    /// </summary>
    public class ShaderItems
    {
        /// <summary>
        /// Шейдер вокселей
        /// </summary>
        public ShaderVoxel ShVoxel { get; protected set; } = new ShaderVoxel();

        public void Create(OpenGL gl)
        {
            ShVoxel.Create(gl, new Dictionary<uint, string> { { 0, "v_position" }, { 1, "v_texCoord" }, { 2, "v_color" } });
        }
    }
}
