using System;
using System.Collections.Generic;
using Godot;
using Addons.ScenePaletter.Core;
using System.Collections.Concurrent;

namespace Addons.ScenePaletter.Tools;

/// <summary>
/// Generates 2D/3D previews of Godot scenes as textures, with caching for performance.
/// Uses a concurrent dictionary to store generated previews and avoid redundant computations.
/// </summary>
/// <remarks>
/// <para>
/// This class provides asynchronous scene preview generation with support for:
/// <list type="bullet">
///   <item>Both 2D (<c>Node2D</c>) and 3D (<c>Node3D</c>) scenes</item>
///   <item>Customizable render size and margins</item>
///   <item>Transparent background option</item>
///   <item>Thread-safe caching of generated textures</item>
/// </list>
/// </para>
/// <para>
/// The generation process:
/// <list type="number">
///   <item>Checks cache for existing preview</item>
///   <item>Creates a <c>SubViewport</c> with the specified settings</item>
///   <item>Instanciates the scene and calculates bounds</item>
///   <item>Sets up appropriate camera (2D/3D) and lighting</item>
///   <item>Renders the scene and extracts the texture</item>
///   <item>Stores the result in cache and invokes the callback</item>
/// </list>
/// </para>
/// <para>Logs via <c>ExceptionHandler.ThrowInvalidResourceTypeException</c> if <c>scene</c> is <c>null</c>.</para>
/// <para>Logs via <c>ExceptionHandler.ThrowSceneInstantiationException</c> if scene instantiation fails.</para>
/// <para>Logs via <c>ExceptionHandler.ThrowPreviewGenerationException</c> if texture generation fails.</para>
/// <para>Logs via <c>ExceptionHandler.ThrowUnexpectedException</c> for any other errors.</para>
/// <para>
/// <example>
/// Generate a preview of a scene:
/// <code>
/// var scene = GD.Load&lt;PackedScene&gt;("res://scenes/my_scene.tscn");
/// ScenePreviewGenerator.GeneratePreview(
///     scene,
///     new Vector2I(512, 512),
///     new Vector2(1.1f, 1.1f),
///     true,
///     (Texture2D texture) => {
///         // Use the generated texture
///         previewTexture.Texture = texture;
///     }
/// );
/// </code>
/// </example>
/// </para>
/// </remarks>
public static class ScenePreviewGenerator
{
    private static ConcurrentDictionary<PackedScene, Texture2D> cache = new();

    /// <summary>
    /// Generates a preview texture of the given scene asynchronously.
    /// </summary>
    /// <param name="scene">The scene to render as preview. Cannot be null.</param>
    /// <param name="size">The size of the output texture in pixels.</param>
    /// <param name="margin">The margin around the scene content (1.0 = no margin, >1.0 = extra space).</param>
    /// <param name="transparent">Whether the background should be transparent.</param>
    /// <param name="action">Callback receiving the generated texture. Can be null.</param>
    /// <remarks>
    /// <para>
    /// If the scene is already in cache, the cached texture is returned immediately.
    /// Otherwise, the scene is instantiated in a SubViewport, rendered, and the resulting
    /// texture is cached and passed to the callback.
    /// </para>
    /// <para>
    /// The method uses Godot's rendering pipeline and waits for the next frame to ensure
    /// the scene is fully rendered before extracting the texture.
    /// </para>
    /// </remarks>
    public static async void GeneratePreview(
        PackedScene scene,
        Vector2I size,
        Vector2 margin,
        bool transparent,
        Action<Texture2D> action)
    {
        if (scene == null)
        {
            ExceptionHandler.ThrowInvalidResourceTypeException(
                "null",
                nameof(PackedScene),
                "null"
            );
            return;
        }

        if (cache.TryGetValue(scene, out var cached))
        {
            action?.Invoke(cached);
            return;
        }

        SubViewport subViewport = null;
        Node instance = null;
        Node awaiter = null;

        try
        {
            subViewport = new SubViewport
            {
                Size = size,
                TransparentBg = transparent,
                OwnWorld3D = true,
                RenderTargetUpdateMode = SubViewport.UpdateMode.Once
            };

            instance = scene.Instantiate();
            if (instance == null)
            {
                ExceptionHandler.ThrowSceneInstantiationException(scene.ResourcePath, nameof(GeneratePreview));
                return;
            }

            subViewport.AddChild(instance);

            if (instance is Node2D node2D)
            {
                SetupRender2D(subViewport, node2D, size, margin);
            }
            else if (instance is Node3D node3D)
            {
                SetupRender3D(subViewport, node3D, size, margin);
            }

            Node root = ((SceneTree)Engine.GetMainLoop()).Root;
            if (root == null)
            {
                ExceptionHandler.ThrowMissingNodeException("SceneTree.Root", nameof(GeneratePreview));
                return;
            }

            root.AddChild(subViewport);

            awaiter = new Node();
            root.AddChild(awaiter);

            await awaiter.ToSignal(root.GetTree(), SceneTree.SignalName.ProcessFrame);
            await awaiter.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);

            Texture2D texture = subViewport.GetTexture();
            if (texture == null)
            {
                ExceptionHandler.ThrowPreviewGenerationException(scene.ResourcePath, nameof(GeneratePreview));
                return;
            }

            Image image = texture.GetImage();
            if (image == null)
            {
                ExceptionHandler.ThrowPreviewGenerationException(scene.ResourcePath, nameof(GeneratePreview));
                return;
            }

            ImageTexture finalTexture = ImageTexture.CreateFromImage(image);
            cache[scene] = finalTexture;
            action?.Invoke(finalTexture);
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, nameof(GeneratePreview));
        }
        finally
        {
            awaiter?.QueueFree();
            subViewport?.QueueFree();
            instance?.QueueFree();
        }
    }

    /// <summary>
    /// Sets up the SubViewport for rendering a 2D scene.
    /// </summary>
    /// <param name="subViewport">The viewport to configure.</param>
    /// <param name="node">The root 2D node of the scene.</param>
    /// <param name="size">The target render size.</param>
    /// <param name="margin">The margin around the content.</param>
    /// <remarks>
    /// <para>
    /// Calculates the bounds of all child nodes and sets up a Camera2D to frame
    /// the entire scene content with the specified margin.
    /// </para>
    /// <para>
    /// Logs via <c>ExceptionHandler.ThrowInvalidPreviewSettingsException</c> if bounds are zero.
    /// </para>
    /// </remarks>
    private static void SetupRender2D(SubViewport subViewport, Node2D node, Vector2I size, Vector2 margin)
    {
        Vector2 minPos = new(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 maxPos = new(float.NegativeInfinity, float.NegativeInfinity);

        Queue<Node> queue = new();
        queue.Enqueue(subViewport);

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (Node child in current.GetChildren())
            {
                queue.Enqueue(child);

                Rect2 rect = GetNodeRect2D(child);
                minPos = minPos.Min(rect.Position - rect.Size / 2);
                maxPos = maxPos.Max(rect.Position + rect.Size / 2);
            }
        }

        Camera2D camera = new Camera2D
        {
            Enabled = true
        };
        subViewport.AddChild(camera);

        Vector2 center = (minPos + maxPos) / 2;
        Vector2 bounds = maxPos - minPos;

        if (bounds == Vector2.Zero)
        {
            ExceptionHandler.ThrowInvalidPreviewSettingsException("2D bounds are zero", nameof(SetupRender2D));
            return;
        }

        camera.Position = center;

        float zoomX = size.X / (bounds.X * margin.X);
        float zoomY = size.Y / (bounds.Y * margin.Y);
        camera.Zoom = Vector2.One * Mathf.Min(zoomX, zoomY);
    }

    /// <summary>
    /// Sets up the SubViewport for rendering a 3D scene.
    /// </summary>
    /// <param name="subViewport">The viewport to configure.</param>
    /// <param name="node">The root 3D node of the scene.</param>
    /// <param name="size">The target render size.</param>
    /// <param name="margin">The margin around the content.</param>
    /// <remarks>
    /// <para>
    /// Calculates the bounds of all child nodes, adds a directional light, and sets up
    /// a Camera3D to frame the entire scene content with the specified margin.
    /// </para>
    /// <para>
    /// Logs via <c>ExceptionHandler.ThrowInvalidPreviewSettingsException</c> if bounds are zero.
    /// </para>
    /// </remarks>
    private static void SetupRender3D(SubViewport subViewport, Node3D node, Vector2I size, Vector2 margin)
    {
        Vector3 minPos = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 maxPos = new(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        Queue<Node> queue = new();
        queue.Enqueue(subViewport);

        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (Node child in current.GetChildren())
            {
                queue.Enqueue(child);

                Aabb aabb = GetNodeRect3D(child);
                minPos = minPos.Min(aabb.Position - aabb.Size / 2);
                maxPos = maxPos.Max(aabb.Position + aabb.Size / 2);
            }
        }

        Vector3 center = (minPos + maxPos) / 2;
        Vector3 bounds = maxPos - minPos;

        if (bounds == Vector3.Zero)
        {
            ExceptionHandler.ThrowInvalidPreviewSettingsException("3D bounds are zero", nameof(SetupRender3D));
            return;
        }

        DirectionalLight3D light = new()
        {
            LightEnergy = 1.0f,
            RotationDegrees = new Vector3(-30f, 120f, 0)
        };
        subViewport.AddChild(light);

        Camera3D camera = new()
        {
            Projection = Camera3D.ProjectionType.Perspective
        };

        Vector3 position =
            center + new Vector3(bounds.X, bounds.Y * 0.75f, bounds.Z)
            * (Mathf.Max(margin.X, margin.Y) - 0.1f);

        subViewport.AddChild(camera);
        camera.LookAtFromPosition(position, center, Vector3.Up);
    }

    /// <summary>
    /// Gets the 2D bounds of a node for preview calculation.
    /// </summary>
    /// <param name="node">The node to measure.</param>
    /// <returns>A rectangle representing the node's bounds.</returns>
    /// <remarks>
    /// <para>
    /// Supports Sprite2D (with texture size and frames), Control (using GetRect()),
    /// and Node2D (using position). Returns zero bounds for unsupported node types.
    /// </para>
    /// </remarks>
    public static Rect2 GetNodeRect2D(Node node)
    {
        if (node is Sprite2D sprite && sprite.Texture != null)
        {
            Vector2 size = sprite.Texture.GetSize() * sprite.Scale
                / new Vector2(sprite.Hframes, sprite.Vframes);

            return new Rect2(sprite.GlobalPosition + sprite.Offset, size);
        }

        if (node is Control control)
            return control.GetRect();

        if (node is Node2D node2D)
            return new Rect2(node2D.Position, Vector2.Zero);

        return new Rect2(Vector2.Zero, Vector2.Zero);
    }

    /// <summary>
    /// Gets the 3D bounds of a node for preview calculation.
    /// </summary>
    /// <param name="node">The node to measure.</param>
    /// <returns>An axis-aligned bounding box representing the node's bounds.</returns>
    /// <remarks>
    /// <para>
    /// Supports MeshInstance3D (with mesh bounds and transform), and Node3D (using position).
    /// Returns zero bounds for unsupported node types.
    /// </para>
    /// </remarks>
    public static Aabb GetNodeRect3D(Node node)
    {
        if (node is MeshInstance3D mesh && mesh.Mesh != null)
        {
            Aabb meshAabb = mesh.Mesh.GetAabb();
            Vector3 center = mesh.Transform.Origin + meshAabb.GetCenter();

            return new Aabb(center, meshAabb.Size * mesh.Scale);
        }

        if (node is Node3D node3D)
            return new Aabb(node3D.Transform.Origin, Vector3.Zero);

        return new Aabb(Vector3.Zero, Vector3.Zero);
    }

    /// <summary>
    /// Clears the preview cache, freeing all stored textures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this when scenes are modified and need to be re-rendered, or to free memory.
    /// </para>
    /// </remarks>
    public static void ClearCache()
    {
        cache.Clear();
    }
}