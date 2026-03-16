using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.BST;

public class BinarySearchTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, BstNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new BstNode<TKey, TValue>(key, value);
    }
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode) { }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child) { }
    
}