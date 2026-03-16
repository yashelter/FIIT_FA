using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    protected virtual (TreapNode<TKey, TValue>?, TreapNode<TKey, TValue>?)
        Split(TreapNode<TKey, TValue>? node, TKey key, bool strict = false)
    {
        if (node is null) return (null, null);

        var cmp = Comparer.Compare(node.Key, key);

        if (strict ? cmp < 0 : cmp <= 0)
        {
            var (left, right) = Split(node.Right, key, strict);
            
            node.Right = left;
            left?.Parent = node;
            node.Parent = null;
            right?.Parent = null;
            
            return (node, right);
        } 
        else 
        {
            var (left, right) = Split(node.Left, key, strict);
            
            node.Left = right;
            right?.Parent = node;
            node.Parent = null;
            left?.Parent = null;
            
            return (left, node);
        }
    }

    protected virtual TreapNode<TKey, TValue>? Merge(
        TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left is null)
        {
            right?.Parent = null;
            
            return right;
        }

        if (right is null)
        {
            left.Parent = null;
            
            return left;
        }

        if (left.Priority >= right.Priority)
        {
            left.Right = Merge(left.Right, right);
            
            if (left.Right is { } leftRight) leftRight.Parent = left;
            
            left.Parent = null;
            
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);

            if (right.Left is { } rightLeft) rightLeft.Parent = right;

            right.Parent = null;

            return right;
        }
    }

    public override void Add(TKey key, TValue value)
    {
        if (FindNode(key) is { } existing)
        {
            existing.Value = value;
            
            return;
        }

        var node = CreateNode(key, value);
        
        var (left, right) = Split(Root, key, strict: true);
        
        Root = Merge(Merge(left, node), right);
        Count++;
        
        OnNodeAdded(node);
    }

    public override bool Remove(TKey key)
    {
        if (!ContainsKey(key)) return false;

        var (less, greaterEq) = Split(Root, key, strict: true);
        var (_, greater) = Split(greaterEq, key, strict: false);

        Root = Merge(less, greater);
        Count--;
        
        OnNodeRemoved(null, null);
        
        return true;
    }
}