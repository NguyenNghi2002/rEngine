﻿using System.Runtime;

namespace Engine
{
    public static class ComponentExt
    {
        public static T AddComponent<T>(this Component cpn, T component, out T newComponent) where T : Component, new()
            => cpn.Entity.AddComponent<T>(component,out newComponent);
        public static T AddComponent<T>(this Component cpn,out T component) where T : Component, new()
            => cpn.Entity.AddComponent<T>(out component);
        public static T AddComponent<T>(this Component cpn) where T : Component, new()
			=> cpn.Entity.AddComponent<T>();
        public static T AddComponent<T>(this Component cpn, T component) where T : Component 
            => cpn.Entity.AddComponent(component);
    }
}
