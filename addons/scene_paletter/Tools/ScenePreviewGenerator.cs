using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace Addons.ScenePaletter.Tools;

public static class ScenePreviewGenerator
{
    public static async void GeneratePreview(PackedScene scene, Vector2I size, Vector2 margin, bool transparent, Action<Texture2D> action)
    {
        // Create viewport with proper settings
        SubViewport subViewport = new SubViewport();
        subViewport.Size = size;
        subViewport.TransparentBg = transparent;
        subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;

        // Instantiate scene
        Node parent = scene.Instantiate();
        subViewport.AddChild(parent);

        Vector2 minPos = Vector2.Zero;
        Vector2 maxPos = Vector2.Zero;

        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(parent);
        while (queue.Count > 0)
        {
            Node current = queue.Dequeue();
            foreach (Node child in current.GetChildren())
            {
                queue.Enqueue(child);

                Rect2 rect = GetNodeRect(child);
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

        subViewport.QueueFree();
        action?.Invoke(imageTexture);
    }

    public static Rect2 GetNodeRect(Node node)
    {
        switch (node)
        {
            case Sprite2D sprite:
                return new Rect2(
                    sprite.GlobalPosition + sprite.Offset,
                    sprite.Texture == null ? Vector2.Zero : sprite.Texture.GetSize() * sprite.Scale);
            case Node2D node2d:
                return new Rect2(node2d.Position, Vector2.Zero);

            case Control control:
                return control.GetRect();

            default:
                return new Rect2(Vector2.Zero, Vector2.Zero);
        }
    }
}