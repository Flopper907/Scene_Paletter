using System;
using System.Collections.Generic;
using Godot;
using Addons.ScenePaletter.Core;
using System.Collections.Concurrent;

namespace Addons.ScenePaletter.Tools;

public static class ScenePreviewGenerator
{
    private static ConcurrentDictionary<PackedScene, Texture2D> cache = new();

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

    // --------------------------------------------------
    // 2D Setup
    // --------------------------------------------------

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

    // --------------------------------------------------
    // 3D Setup
    // --------------------------------------------------

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

    // --------------------------------------------------
    // Bounds helpers
    // --------------------------------------------------

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


    // --------------------------------------------------
    // Cache
    // --------------------------------------------------

    public static void ClearCache()
    {
        cache.Clear();
    }
}