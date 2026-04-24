using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        // не надо
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node == null)
        {
            value = default;
            return false;
        }
        else
        {
            Splay(node);
            value = node.Value;
            return true;
        }

    }
    protected override void RemoveNode(BstNode<TKey, TValue> node)
    {
        Splay(node);
        
        base.RemoveNode(node);
    }
    private void Splay(BstNode<TKey, TValue> node)
    {
        while (node.Parent != null)  // Пока не станет корнем
        {
            BstNode<TKey, TValue> parent = node.Parent;
            BstNode<TKey, TValue>? grandParent = parent.Parent;
        
            if (grandParent == null)
            {
                // Zig: родитель - корень
                if (node.IsLeftChild)
                    RotateRight(node);  // ваш метод
                else
                    RotateLeft(node);   // ваш метод
            }
            else if (node.IsLeftChild && parent.IsLeftChild)
            {
                // Zig-Zig: оба левые
                RotateRight(parent);    // сначала поднимаем родителя
                RotateRight(node);      // потом сам узел
            }
            else if (node.IsRightChild && parent.IsRightChild)
            {
                // Zig-Zig: оба правые
                RotateLeft(parent);     // сначала поднимаем родителя
                RotateLeft(node);       // потом сам узел
            }
            else if (node.IsRightChild && parent.IsLeftChild)
            {
                // Zig-Zag: node справа, parent слева
                RotateLeft(node);       // поднимаем node
                RotateRight(node);      // ещё раз поднимаем node
            }
            else // node.IsLeftChild && parent.IsRightChild
            {
                // Zig-Zag: node слева, parent справа
                RotateRight(node);      // поднимаем node
                RotateLeft(node);       // ещё раз поднимаем node
            }
        }
    
        // После всех поворотов node стал корнем
        this.Root = node;
    }

    public override bool ContainsKey(TKey key)
    {
        BstNode<TKey, TValue>? node = FindNode(key);
        if (node == null)
        {
            return false;
        }
        Splay(node);
        return true;
    }
}
