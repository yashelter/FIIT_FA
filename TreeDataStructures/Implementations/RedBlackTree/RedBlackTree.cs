using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    
    private static bool IsRed(RbNode<TKey, TValue>? node) => 
        node?.Color == RbColor.Red;
    
    
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value) { Color = RbColor.Red };
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        FixAfterInsert(newNode);
    }
    
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        var nodeToFix = child ?? parent;
        if (nodeToFix is not null)
        {
            FixAfterDelete(nodeToFix);
        }
    }
    
    
    private void FixAfterInsert(RbNode<TKey, TValue> node)
    {
        node.Color = RbColor.Red;
        
        while (true)
        {
            var parent = node.Parent;
            if (parent is null || parent.Color == RbColor.Black)
                break;
            
            var grandparent = parent.Parent;
            if (grandparent is null)
                break;
            
            if (parent.IsLeftChild)
            {
                var uncle = grandparent.Right;
                
                if (IsRed(uncle))
                {
                    parent.Color = RbColor.Black;
                    if (uncle is not null) uncle.Color = RbColor.Black;
                    grandparent.Color = RbColor.Red;
                    node = grandparent;
                }
                else
                {
                    if (node.IsRightChild)
                    {
                        RotateLeft(parent);
                        node = parent;
                        parent = node.Parent!;
                        grandparent = parent.Parent!;
                    }
                    RotateRight(grandparent);
                    parent.Color = RbColor.Black;
                    grandparent.Color = RbColor.Red;
                    break;
                }
            }
            else
            {
                var uncle = grandparent.Left;
                
                if (IsRed(uncle))
                {
                    parent.Color = RbColor.Black;
                    if (uncle is not null) uncle.Color = RbColor.Black;
                    grandparent.Color = RbColor.Red;
                    node = grandparent;
                }
                else
                {
                    if (node.IsLeftChild)
                    {
                        RotateRight(parent);
                        node = parent;
                        parent = node.Parent!;
                        grandparent = parent.Parent!;
                    }
                    RotateLeft(grandparent);
                    parent.Color = RbColor.Black;
                    grandparent.Color = RbColor.Red;
                    break;
                }
            }
        }
        
        if (Root is not null)
            Root.Color = RbColor.Black;
    }
    
    
    private void FixAfterDelete(RbNode<TKey, TValue> node)
    {
        RbNode<TKey, TValue>? current = node;
        
        while (current != Root && current.Color == RbColor.Black)
        {
            if (current.IsLeftChild)
            {
                var sibling = current.Parent?.Right;
                
                if (IsRed(sibling))
                {
                    if (sibling is not null) sibling.Color = RbColor.Black;
                    if (current.Parent is not null) current.Parent.Color = RbColor.Red;
                    if (current.Parent is not null) RotateLeft(current.Parent);
                    sibling = current.Parent?.Right;
                }
                
                if (sibling is null || (!IsRed(sibling.Left) && !IsRed(sibling.Right)))
                {
                    if (sibling is not null) sibling.Color = RbColor.Red;
                    current = current.Parent;
                }
                else
                {
                    if (!IsRed(sibling.Right))
                    {
                        if (sibling.Left is not null) sibling.Left.Color = RbColor.Black;
                        sibling.Color = RbColor.Red;
                        RotateRight(sibling);
                        sibling = current.Parent?.Right;
                    }
                    
                    if (sibling is not null && current.Parent is not null)
                    {
                        sibling.Color = current.Parent.Color;
                        current.Parent.Color = RbColor.Black;
                        if (sibling.Right is not null) sibling.Right.Color = RbColor.Black;
                    }
                    if (current.Parent is not null) RotateLeft(current.Parent);
                    current = Root;
                }
            }
            else
            {
                var sibling = current.Parent?.Left;
                
                if (IsRed(sibling))
                {
                    if (sibling is not null) sibling.Color = RbColor.Black;
                    if (current.Parent is not null) current.Parent.Color = RbColor.Red;
                    if (current.Parent is not null) RotateRight(current.Parent);
                    sibling = current.Parent?.Left;
                }
                
                if (sibling is null || (!IsRed(sibling.Right) && !IsRed(sibling.Left)))
                {
                    if (sibling is not null) sibling.Color = RbColor.Red;
                    current = current.Parent;
                }
                else
                {
                    if (!IsRed(sibling.Left))
                    {
                        if (sibling.Right is not null) sibling.Right.Color = RbColor.Black;
                        sibling.Color = RbColor.Red;
                        RotateLeft(sibling);
                        sibling = current.Parent?.Left;
                    }
                    
                    if (sibling is not null && current.Parent is not null)
                    {
                        sibling.Color = current.Parent.Color;
                        current.Parent.Color = RbColor.Black;
                        if (sibling.Left is not null) sibling.Left.Color = RbColor.Black;
                    }
                    if (current.Parent is not null) RotateRight(current.Parent);
                    current = Root;
                }
            }
            
            if (current is null) break;
        }
        
        if (current is not null)
            current.Color = RbColor.Black;
    }
    
}