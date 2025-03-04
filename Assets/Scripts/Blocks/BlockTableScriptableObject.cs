using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Block Table")]
public class BlockTableScriptableObject : ScriptableObject
{
    public List<Block> blocks = new List<Block>();

    
    public Block GetItem(string id)
    {
        return this.blocks.Find(item => item.id == id);
    }

    public BlockScriptableObject GetBlock(string id)
    {
        var block = this.GetItem(id);
        return block.block as BlockScriptableObject;
    }

    //
    public string GetBlock(BlockScriptableObject blockSO)
    {
        foreach (var block in blocks)
        {
            if (block.block == blockSO)
            {
                return block.id;
            }
        }
        return null;
    }
    //





    private Texture2D blocksAtlas = null;
    //private Texture2D itemsAtlas = null;

    public Texture2D GetBlockAtlasTexture()
    {
        return this.blocksAtlas;
    }

    /*
    public Texture2D GetItemAtlasTexture()
    {
        return this.itemsAtlas;
    }
    */

    public void GenerateTextureAtlas()
    {
        this.blocksAtlas = new Texture2D(8192, 8192)
        {
            filterMode = FilterMode.Point,
        };

        /*this.itemsAtlas = new Texture2D(8192, 8192)
        {
            filterMode = FilterMode.Point,
        };*/

        List<Texture2D> blocksTextures = new List<Texture2D>();
        //List<Texture2D> itemsTextures = new List<Texture2D>();


        foreach (var block in blocks)
        {

            var varblock = block.block as BlockScriptableObject;

            var blockTextures = varblock.GetTextures();
            blocksTextures.AddRange(blockTextures);
        }
        

        List<Rect> blocksRects = this.blocksAtlas.PackTextures(blocksTextures.ToArray(), 0, 8192).ToList();
        //List<Rect> itemsRects = this.itemsAtlas.PackTextures(itemsTextures.ToArray(), 0, 8192).ToList();

        foreach (var block in blocks)
        {
            var varblock = block.block as BlockScriptableObject;
            varblock.rects = blocksRects.GetRange(0, 6).ToArray();
            blocksRects.RemoveRange(0, 6);
        }
    }
}
