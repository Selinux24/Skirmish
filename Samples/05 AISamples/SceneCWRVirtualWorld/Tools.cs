using Engine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AISamples.SceneCWRVirtualWorld
{
    class Tools(Scene scene, World world)
    {
        private readonly Scene scene = scene;
        private readonly World world = world;
        private readonly Dictionary<EditorModes, IEditor> editors = [];
        private EditorModes editorMode;
        private IEditor currentEditor;

        public EditorModes[] GetModes()
        {
            return [.. editors.Keys];
        }

        public void AddEditor<T>(EditorModes mode, float height) where T : IEditor
        {
            var editor = (T)Activator.CreateInstance(typeof(T), [world, height]);

            if (editors.TryAdd(mode, editor))
            {
                return;
            }

            editors[mode] = editor;
        }
        public async Task Initialize()
        {
            foreach (var item in editors.Values)
            {
                await item.Initialize(scene);
            }
        }

        public void SetEditor(EditorModes mode)
        {
            DisableAllEditors();

            if (mode == EditorModes.None)
            {
                editorMode = EditorModes.None;
                currentEditor = null;

                return;
            }

            if (editorMode != EditorModes.None && editorMode == mode)
            {
                editorMode = EditorModes.None;
                currentEditor = null;

                return;
            }

            editorMode = mode;

            currentEditor = editors[editorMode];
            currentEditor.Visible = true;
        }
        private void DisableAllEditors()
        {
            foreach (var item in editors)
            {
                item.Value.Visible = false;
            }
        }

        public void Update(IGameTime gameTime)
        {
            if (editorMode == EditorModes.None)
            {
                return;
            }

            if (currentEditor == null)
            {
                return;
            }

            currentEditor.UpdateInputEditor(gameTime);
        }
        public void Draw()
        {
            if (currentEditor == null)
            {
                return;
            }

            currentEditor.Draw();
        }
    }
}
