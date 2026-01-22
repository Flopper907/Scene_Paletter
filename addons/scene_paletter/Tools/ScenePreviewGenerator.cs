using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace Addons.ScenePaletter.Tools;

public static class ScenePreviewGenerator
{
    private static Dictionary<PackedScene, Texture2D> cache = new Dictionary<PackedScene, Texture2D>();

    public static async void GeneratePreview(PackedScene scene, Vector2I size, Vector2 margin, bool transparent, Action<Texture2D> action)
    {
        if (cache.ContainsKey(scene))
        {
            action?.Invoke(cache[scene]);
            return;
        }

        // Create viewport with proper settings
        SubViewport subViewport = new SubViewport();
        subViewport.Size = size;
        subViewport.TransparentBg = transparent;
        subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;

        // Instantiate scene
        Node node = scene.Instantiate();
        subViewport.AddChild(node);

        if(node is Node2D node2D)
        {
            SetupRender2D(subViewport,node2D, size, margin);
        }
        else if(node is Node3D node3D)
        {
            SetupRender3D(subViewport,node3D, size, margin);
        }

        Node root = ((SceneTree)Engine.GetMainLoop()).Root;
        root.AddChild(subViewport);


        Node awaiter = new Node();
        root.AddChild(awaiter);

        await awaiter.ToSignal(root.GetTree(), SceneTree.SignalName.ProcessFrame);
        await awaiter.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);

        awaiter.Free();

        Texture2D texture = subViewport.GetTexture();
        Image image = texture.GetImage();
        ImageTexture imageTexture = ImageTexture.CreateFromImage(image);

        subViewport.Free();
        cache[scene] = imageTexture;
        action?.Invoke(imageTexture);
    }
    private static void SetupRender3D(SubViewport subViewport, Node3D node, Vector2I size, Vector2 margin)
    {
        Camera3D camera = new Camera3D();
        subViewport.AddChild(camera);
    }

    private static void SetupRender2D(SubViewport subViewport, Node2D node, Vector2I size, Vector2 margin)
    {
    
        Vector2 minPos = Vector2.Zero;
        Vector2 maxPos = Vector2.Zero;

        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(node);
        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (Node child in current.GetChildren())
            {
                queue.Enqueue(child);

                Rect2 rect = GetNodeRect2D(child);
                minPos.X = Mathf.Min(minPos.X, rect.Position.X - rect.Size.X / 2);
                minPos.Y = Mathf.Min(minPos.Y, rect.Position.Y - rect.Size.Y / 2);
                maxPos.X = Mathf.Max(maxPos.X, rect.Position.X + rect.Size.X / 2);
                maxPos.Y = Mathf.Max(maxPos.Y, rect.Position.Y + rect.Size.Y / 2);
            }
        }

        // Add camera to frame the content
        Camera2D camera = new Camera2D();
        camera.Enabled = true;
        subViewport.AddChild(camera);

        // Center camera on content
        Vector2 center = (minPos + maxPos) / 2;
        Vector2 bounds = maxPos - minPos;
        camera.Position = center;

        // Adjust zoom to fit content
        float zoomX = size.X / (bounds.X * margin.X);
        float zoomY = size.Y / (bounds.Y * margin.Y);
        camera.Zoom = Vector2.One * Mathf.Min(zoomX, zoomY);
    }

    public static Rect2 GetNodeRect2D(Node node)
    {
        switch (node)
        {
            case Sprite2D sprite:
                return new Rect2(
                    sprite.GlobalPosition + sprite.Offset,
                    sprite.Texture == null ? Vector2.Zero : sprite.Texture.GetSize() * sprite.Scale / new Vector2(sprite.Hframes, sprite.Vframes));
            case Node2D node2d:
                return new Rect2(node2d.Position, Vector2.Zero);

            case Control control:
                return control.GetRect();

            default:
                return new Rect2(Vector2.Zero, Vector2.Zero);
        }
    }

    public static void ClearCache()
    {
        cache = new Dictionary<PackedScene, Texture2D>();
    }
}