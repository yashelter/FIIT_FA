using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> 
    : BinarySearchTreeBase<TKey, TValue, SplayNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override SplayNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(SplayNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(SplayNode<TKey, TValue>? deletedNode, 
                                           SplayNode<TKey, TValue>? replacement)
    {
        var nodeToSplay = replacement ?? deletedNode?.Parent;
        if (nodeToSplay is not null)
        {
            Splay(nodeToSplay);
        }
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        
        if (node is not null)
        {
            Splay(node);
            value = node.Value;
            return true;
        }
        
        value = default;
        return false;
    }
    
    public override bool ContainsKey(TKey key)
    {
        var node = FindNode(key);
        if (node is not null)
        {
            Splay(node);
            return true;
        }
        return false;
    }
    
    private void Splay(SplayNode<TKey, TValue> node)
    {
        while (node.Parent is not null)
        {
            var parent = node.Parent;
            var grandparent = parent.Parent;
            
            if (grandparent is null)
            {
                if (node.IsLeftChild)
                    RotateRight(parent);
                else
                    RotateLeft(parent);
            }
            else if (node.IsLeftChild && parent.IsLeftChild)
            {
                RotateRight(grandparent);
                RotateRight(parent);
            }
            else if (node.IsRightChild && parent.IsRightChild)
            {
                RotateLeft(grandparent);
                RotateLeft(parent);
            }
            else if (node.IsLeftChild && parent.IsRightChild)
            {
                RotateRight(parent);
                RotateLeft(grandparent);
            }
            else if (node.IsRightChild && parent.IsLeftChild)
            {
                RotateLeft(parent);
                RotateRight(grandparent);
            }
        }
        
        Root = node;
    }
}