using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    where TKey : IComparable<TKey>
{
    public override void Add(TKey key, TValue value)
    {
        var node = FindAndSplay(key, out var found);
        
        if (!found)
        {
            base.Add(key, value);

            return;
        }

        node!.Value = value;
        
        Splay(node);
    }

    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode) => Splay(newNode);

    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if ((child ?? parent) is { } node)
        {
            Splay(node);
        }
    }

    public override bool ContainsKey(TKey key) => FindAndSplay(key, out var found) != null && found;

    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindAndSplay(key, out var found);
        
        if (!found || node == null)
        {
            value = default;

            return false;
        }

        value = node.Value;

        return true;
    }

    private BstNode<TKey, TValue>? FindAndSplay(TKey key, out bool found)
    {
        BstNode<TKey, TValue>? current = Root, node = null;

        while (current != null)
        {
            node = current;

            var cmp = Comparer.Compare(key, current.Key);
            
            if (cmp == 0)
            {
                found = true;
                
                Splay(current);

                return current;
            }

            current = cmp < 0 ? current.Left : current.Right;
        }

        found = false;
        
        if (node != null)
        {
            Splay(node);
        }

        return node;
    }

    private void Splay(BstNode<TKey, TValue> node)
    {
        while (node.Parent is { } parent)
        {
            // zig
            if (parent.Parent is not { } grand)
            {
                if (node.IsLeftChild)
                {
                    RotateRight(parent);
                }
                else
                {
                    RotateLeft(parent);
                }
            }
            // zig-zig
            else if (node.IsLeftChild && parent.IsLeftChild)
            {
                RotateRight(grand);
                RotateRight(parent);
            }
            else if (node.IsRightChild && parent.IsRightChild)
            {
                RotateLeft(grand);
                RotateLeft(parent);
            }
            // zig-zag
            else if (node.IsRightChild)
            {
                RotateLeft(parent);
                RotateRight(grand);
            }
            else
            {
                RotateRight(parent);
                RotateLeft(grand);
            }
        }
    }
}
