#if false
using Raylib_cs;

namespace Engine
{
    public class rSound : IDisposable
    {
        Sound _sound;
        public AudioStream Stream
        {
            get => _sound.stream;
            set => _sound.stream = value;
        }
        public rSound(Sound sound)
        {
            _sound = sound;
        }
        public void Dispose()
        {
            Raylib.UnloadSound(_sound);
        }

        public static implicit operator Sound(rSound rFont) => rFont._sound;
        public static rSound Load(string filePath)
        {

            Insist.IsTrue(Path.HasExtension(filePath), $"{filePath} doesn't have extension");
            var a = Raylib.LoadSound(filePath);
            return new rSound(a);
        }


    }


} 
#endif