using System;
using UObject = UnityEngine.Object;

namespace Starlight.Prism.Lib;

/// <summary>
/// A change to make to a created prefab's components
/// </summary>
public abstract class ComponentEdit
{
    /// <summary>
    /// Applies this change to the target
    /// </summary>
    public abstract void ApplyTo(GameObject target);

    /// <summary>
    /// Adds a component, optionally configuring it
    /// </summary>
    /// <remarks> Does nothing if one is already present </remarks>
    public static ComponentEdit Add<T>(Action<T>? configure = null) where T : Component =>
        new AddEdit<T>(configure);

    /// <summary>
    /// Removes every component of this type, including on children
    /// </summary>
    public static ComponentEdit Remove<T>() where T : Component => new RemoveEdit<T>();

    /// <summary>
    /// Configures an existing component
    /// </summary>
    /// <remarks> Does nothing if none is present </remarks>
    public static ComponentEdit Configure<T>(Action<T> configure) where T : Component =>
        new ConfigureEdit<T>(configure);

    /// <summary>
    /// Applies a list of changes
    /// </summary>
    public static void ApplyEdits(GameObject target, IEnumerable<ComponentEdit> edits)
    {
        ArgumentNullException.ThrowIfNull(target);

        foreach (var edit in edits)
        {
            try
            {
                edit.ApplyTo(target);
            }
            catch (Exception ex)
            {
                LogError($"Prism: Component edit {edit.GetType().Name} failed on '{target.name}'", ex);
            }
        }
    }

    private sealed class AddEdit<T> : ComponentEdit where T : Component
    {
        private readonly Action<T>? _configure;

        public AddEdit(Action<T>? configure) => _configure = configure;

        public override void ApplyTo(GameObject target)
        {
            var component = target.GetComponent<T>() ?? target.AddComponent<T>();
            _configure?.Invoke(component);
        }
    }

    private sealed class RemoveEdit<T> : ComponentEdit where T : Component
    {
        public override void ApplyTo(GameObject target)
        {
            foreach (var component in target.GetComponentsInChildren<T>(includeInactive: true))
                UObject.DestroyImmediate(component);
        }
    }

    private sealed class ConfigureEdit<T> : ComponentEdit where T : Component
    {
        private readonly Action<T> _configure;

        public ConfigureEdit(Action<T> configure) => _configure = configure;

        public override void ApplyTo(GameObject target)
        {
            var component = target.GetComponent<T>();
            if (component == null) return;
            _configure(component);
        }
    }
}