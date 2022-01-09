using MvkServer.Glm;

namespace MvkServer.Entity
{
    /// <summary>
    /// Получить размеры наименьшие и наибольшие
    /// </summary>
    public struct HitBoxSizeUD
    {
        private vec3 vd;
        private vec3 vu;
        private vec3i vdi;
        private vec3i vui;

        public HitBoxSizeUD(vec3 pos, HitBox size)
        {
            float w = size.GetWidth();
            float h = size.GetHeight();

            vd = new vec3(pos.x - w, pos.y, pos.z - w);
            vu = new vec3(pos.x + w, pos.y + h, pos.z + w);
            vdi = new vec3i(vd);
            vui = new vec3i(vu);
        }

        public vec3 GetVd() => vd;
        public vec3 GetVu() => vu;
        public vec3i GetVdi() => vdi;
        public vec3i GetVui() => vui;
    }
}
