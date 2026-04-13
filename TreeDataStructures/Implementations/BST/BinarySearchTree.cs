using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.BST;

public class BinarySearchTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, BstNode<TKey, TValue>>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        BstNode<TKey, TValue> node = new BstNode<TKey, TValue>(key, value);
        return node;
    }
    
}