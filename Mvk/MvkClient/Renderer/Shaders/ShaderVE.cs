using SharpGL;
using SharpGL.Shaders;
using System.Collections.Generic;

namespace MvkClient.Renderer.Shaders
{
    /// <summary>
    /// Наследуемый объект шедера
    /// </summary>
    public class ShaderVE : ShaderProgram
    {
        protected virtual string _VertexShaderSource { get; }
        protected virtual string _FragmentShaderSource { get; }

        public ShaderVE() { }

        public void Create(OpenGL gl, Dictionary<uint, string> attributeLocations) 
            => Create(gl, _VertexShaderSource, _FragmentShaderSource, attributeLocations);
    }
}
