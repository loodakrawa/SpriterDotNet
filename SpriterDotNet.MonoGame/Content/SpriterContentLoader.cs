// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpriterDotNet.MonoGame.Sprites;
using SpriterDotNet.Providers;
using System.Collections.Generic;

namespace SpriterDotNet.MonoGame.Content
{
    public class SpriterContentLoader
    {
        public Spriter Spriter { get; private set; }

        private readonly ContentManager content;
        private readonly string scmlPath;
        private readonly string rootPath;

        private Dictionary<int, TexturePackerSheet> atlases;
        private Dictionary<TexturePackerSheet, Dictionary<string, ImageInfo>> infos;

        public SpriterContentLoader(ContentManager content, string scmlPath)
        {
            this.content = content;
            this.scmlPath = scmlPath;
            rootPath = scmlPath.Substring(0, scmlPath.LastIndexOf("/"));
        }

        public void Fill(DefaultProviderFactory<ISprite, SoundEffect> factory)
        {
            if (Spriter == null) Load();

            foreach (SpriterFolder folder in Spriter.Folders)
            {
                if (atlases != null && atlases.Count > 0) AddAtlasFolder(folder, factory);
                else AddRegularFolder(folder, factory);
            }
        }

        private void AddRegularFolder(SpriterFolder folder, DefaultProviderFactory<ISprite, SoundEffect> factory)
        {
            foreach (SpriterFile file in folder.Files)
            {
                string path = FormatPath(file.Name);

                if (file.Type == SpriterFileType.Sound)
                {
                    SoundEffect sound = LoadContent<SoundEffect>(path);
                    factory.SetSound(Spriter, folder, file, sound);
                }
                else
                {
                    Texture2D texture = LoadContent<Texture2D>(path);
                    TextureSprite sprite = new TextureSprite(texture);
                    factory.SetSprite(Spriter, folder, file, sprite);
                }

            }
        }

        private void AddAtlasFolder(SpriterFolder folder, DefaultProviderFactory<ISprite, SoundEffect> factory)
        {
            int id = folder.AtlasId;
            TexturePackerSheet atlas = atlases[id];
            Texture2D texture = LoadContent<Texture2D>(FormatPath(atlas.Meta.Image));
            Dictionary<string, ImageInfo> imageInfos = infos[atlas];

            foreach (SpriterFile file in folder.Files)
            {
                ImageInfo info = imageInfos[file.Name];

                // "x", "y" = location in spritesheet, "w", "h" = trimmed unrotated image size
                Size frame = info.Frame;

                // "w", "h" = original image size﻿
                Size source = info.SourceSize;

                // "x", "y" = trimmed offset - pixels trimmed from the top and left
                Size spriteSource = info.SpriteSourceSize;

                Rectangle sourceRectangle;
                bool rotated = false;

                if (info.Rotated)
                {
                    sourceRectangle = new Rectangle(frame.X, frame.Y, frame.H, frame.W);
                    rotated = true;
                }
                else
                {
                    sourceRectangle = new Rectangle(frame.X, frame.Y, frame.W, frame.H);
                }

                float trimLeft = spriteSource.X;
                float trimRight = source.W - frame.W - spriteSource.X;
                float trimTop = spriteSource.Y;
                float trimBottom = source.H - frame.H - spriteSource.Y;

                int width = source.W;
                int height = source.H;

                TexturePackerSprite sprite = new TexturePackerSprite(texture, sourceRectangle, width, height, rotated, trimLeft, trimRight, trimTop, trimBottom);

                factory.SetSprite(Spriter, folder, file, sprite);
            }
        }

        private void Load()
        {
            Spriter = LoadContent<Spriter>(scmlPath);
            if (Spriter.Atlases == null || Spriter.Atlases.Length == 0) return;
            atlases = new Dictionary<int, TexturePackerSheet>();
            infos = new Dictionary<TexturePackerSheet, Dictionary<string, ImageInfo>>();

            foreach (var atlasRef in Spriter.Atlases)
            {
                string path = FormatPath(atlasRef.Name);
                TexturePackerSheet atlas = LoadContent<TexturePackerSheet>(path);
                atlases[atlasRef.Id] = atlas;

                Dictionary<string, ImageInfo> imageInfos = new Dictionary<string, ImageInfo>();
                infos[atlas] = imageInfos;

                foreach (ImageInfo info in atlas.ImageInfos) imageInfos[info.Name] = info;
            }
        }

        private string FormatPath(string fileName)
        {
            return string.Format("{0}/{1}", rootPath, fileName);
        }

        private T LoadContent<T>(string path)
        {
            int index = path.LastIndexOf(".");
            if (index >= 0) path = path.Substring(0, index);

            T asset = default(T);
            try
            {
                asset = content.Load<T>(path);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Missing Asset: " + path);
            }

            return asset;
        }
    }
}

