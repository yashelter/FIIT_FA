using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Splay;

public class SplayNode<TKey, TValue>(TKey key, TValue value)
    : Node<TKey, TValue, SplayNode<TKey, TValue>>(key, value)
{
    
}