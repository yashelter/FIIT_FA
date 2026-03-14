using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.BST;

public class BinarySearchTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, BstNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value) =>
        new BstNode<TKey, TValue>(key, value);
}