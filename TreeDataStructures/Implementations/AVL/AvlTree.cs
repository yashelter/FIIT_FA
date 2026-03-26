using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value) => 
        new(key, value);

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode) => 
        RebalanceUpward(newNode.Parent);

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child) => 
        RebalanceUpward(parent);

    private void RebalanceUpward(AvlNode<TKey, TValue>? node)
    {
        while (node != null)
        {
            UpdateHeight(node);
            
            var balance = BalanceFactor(node);
            var parent = node.Parent;
            var newNode = node;

            switch (balance)
            {
                case > 1:
                {
                    var left = node.Left!;
                
                    if (BalanceFactor(left) < 0)
                    {
                        RotateLeft(left);
                        UpdateHeight(left);
                        UpdateHeight(left.Parent!);
                    }
                
                    RotateRight(node);
                    UpdateHeight(node);
                    UpdateHeight(node.Parent!);
                
                    newNode = node.Parent!;
                    
                    break;
                }
                case < -1:
                {
                    var right = node.Right!;
                
                    if (BalanceFactor(right) > 0)
                    {
                        RotateRight(right);
                        UpdateHeight(right);
                        UpdateHeight(right.Parent!);
                    }
                
                    RotateLeft(node);
                    UpdateHeight(node);
                    UpdateHeight(node.Parent!);
                
                    newNode = node.Parent!;
                    
                    break;
                }
            }

            node = newNode.Parent;
        }
    }

    private static int Height(AvlNode<TKey, TValue>? node) => 
        node?.Height ?? 0;

    private static void UpdateHeight(AvlNode<TKey, TValue> node) =>
        node.Height = Math.Max(Height(node.Left), Height(node.Right)) + 1;

    private static int BalanceFactor(AvlNode<TKey, TValue> node) =>
        Height(node.Left) - Height(node.Right);
}
