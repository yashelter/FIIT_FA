using TreeDataStructures.Core;
using TreeDataStructures.Implementations.Treap;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        AvlNode<TKey, TValue> current = newNode.Parent;
        while (current != null)
        {
            BalanceTree(current);
            current = current.Parent;
        }
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        AvlNode<TKey, TValue> current;
        if (child != null)
        {
            current = child;
        }
        else
        {
            current = parent;
        }

        while (current != null)
        {
            BalanceTree(current);
            current = current.Parent;
        }
    }

    protected void BalanceTree(AvlNode<TKey, TValue> node)
    {
        UpdateHeight(node);

        int balance = BalancingCheckout(node);

        if (balance < -1)
        {
            if (BalancingCheckout(node.Right) <= 0) 
            {
                RotateLeft(node);
            }
            else if (BalancingCheckout(node.Right) > 0)
            {
                RotateBigLeft(node);
            }
        }
        else if (balance > 1)
        {
            if (BalancingCheckout(node.Left) >= 0)
            {
                RotateRight(node);
            }
            else if (BalancingCheckout(node.Left) < 0)
            {
                RotateBigRight(node);
            }
        }

        UpdateHeight(node);
    }
    private int GetHeight(AvlNode<TKey, TValue>? node)
    => node?.Height ?? 0;

    private void UpdateHeight(AvlNode<TKey, TValue> node)
    {
        int leftHeight = GetHeight(node.Left);
        int rightHeight = GetHeight(node.Right);

        node.Height = 1 + Math.Max(rightHeight, leftHeight);
    }

    private int BalancingCheckout(AvlNode<TKey, TValue> node)
        => GetHeight(node.Left) - GetHeight(node.Right);
}