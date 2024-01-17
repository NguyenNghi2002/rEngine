using Raylib_cs;

namespace Engine
{
    public class rShader 
    {
        private Shader _shader;

        public void SetUniform<T>(string locName,T value,ShaderUniformDataType shaderUniform)
            where T : unmanaged
        {
            int loc = Raylib.GetShaderLocation(_shader,locName);
            Raylib.SetShaderValue<T>(_shader, loc, value, shaderUniform);
        }
        public void SetUniform<T>(string locName, T[] value, ShaderUniformDataType shaderUniform)
            where T : unmanaged
        {
            var loc = Raylib.GetShaderLocation(_shader, locName);
            Raylib.SetShaderValue<T>(_shader, loc, value, shaderUniform);
        }
        public void Dispose()
        {
            if(_shader.id != 0)
            {
                Raylib.UnloadShader(_shader) ;
            }
        }

        public static implicit operator Shader(rShader shader) 
            => shader._shader;
    }
}