using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    private void BalanceAvlTree(AvlNode<TKey, TValue> currentNode)
    {
        int balance = GetBalance(currentNode);
    
        // левое поддерево больше 
        if (balance > 1)
        {
            // LR случай - правое поддерево левого ребенка тяжелее
            if (GetBalance(currentNode.Left!) < 0)
            {
                RotateLeft(currentNode.Left!.Right!);
                UpdateHeight(currentNode.Left);
            }
            RotateRight(currentNode.Left!);
            UpdateHeight(currentNode);
        }
        // правое поддерево больше (balance < -1)
        else if (balance < -1)
        {
            // RL случай - левое поддерево правого ребенка тяжелее
            if (GetBalance(currentNode.Right!) > 0)
            {
                RotateRight(currentNode.Right!.Left!);
                UpdateHeight(currentNode.Right);
            }
            RotateLeft(currentNode.Right!);
            UpdateHeight(currentNode);
        }
    }

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        UpdateHeight(newNode);
        AvlNode<TKey, TValue>? current = newNode.Parent;
        
        while (current != null)
        {
            int oldHeight = current.Height;
            UpdateHeight(current);
            
            // Если высота не изменилась, то выше тоже всё стабильно
            if (oldHeight == current.Height && Math.Abs(GetBalance(current)) < 2)
                break;
            
            BalanceAvlTree(current);
            
            current = current.Parent;
        }
    }

    protected override void RemoveNode(AvlNode<TKey, TValue> node)
    {
        
        
        // у узла нет детей
        if (node.Left == null && node.Right == null)
        {
            if (node.IsLeftChild) node.Parent!.Left = null;
            else if (node.IsRightChild) node.Parent!.Right = null;
            else Root = null;
            
            OnNodeRemoved(node.Parent, null);
        }
        // у узла есть только правый ребенок
        else if (node.Left == null && node.Right != null)
        {
            Transplant(node, node.Right);
            OnNodeRemoved(node.Parent, node.Right);
        }
        // у узла есть только левый ребенок
        else if (node.Left != null && node.Right == null)
        {
            Transplant(node, node.Left);
            OnNodeRemoved(node.Parent, node.Left);
        }
        // два ребенка у узла
        else if (node.Left != null && node.Right != null)
        {
            var successor = FindMin(node.Right);
            
            node.Key = successor.Key;
            node.Value = successor.Value;
            
            // Удаляем successor
            if (successor.IsLeftChild)
                successor.Parent!.Left = successor.Right;
            else
                successor.Parent!.Right = successor.Right;
                
            if (successor.Right != null)
                successor.Right.Parent = successor.Parent;
            
            OnNodeRemoved(successor.Parent, successor.Right);
        }
        else
        {
            throw new InvalidOperationException("Internal Error!");
        }
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        // Нужно запустить балансировку от родителя удаляемого узла
        if (parent == null) return;
    
        var current = parent;
        while (current != null)
        {
            int oldHeight = current.Height;
            UpdateHeight(current);
            
            // Если высота не изменилась и баланс в норме, можно остановиться
            if (oldHeight == current.Height && Math.Abs(GetBalance(current)) < 2)
            {
                // Но нужно проверить, что выше тоже всё норм
                if (IsBalancedUpToRoot(current))
                    break;
            }
            
            BalanceAvlTree(current);
            
            current = current.Parent;
        }
    }

    protected override void RotateLeft(AvlNode<TKey, TValue> x)
    {
        base.RotateLeft(x);
        
        if (x.Left != null) UpdateHeight(x.Left);
        UpdateHeight(x);
        if (x.Parent != null) UpdateHeight(x.Parent);
    }

    protected override void RotateRight(AvlNode<TKey, TValue> y)
    {
        base.RotateRight(y);
        
        if (y.Right != null) UpdateHeight(y.Right);
        UpdateHeight(y);
        if (y.Parent != null) UpdateHeight(y.Parent);
    }
    
    private AvlNode<TKey, TValue> FindMin(AvlNode<TKey, TValue> node)
    {
        while (node.Left != null) node = node.Left;
        return node;
    }

    private void UpdateHeight(AvlNode<TKey, TValue> node)
    {
        node.Height = Math.Max(node.Left?.Height ?? 0, node.Right?.Height ?? 0) + 1;
    }

    private int GetBalance(AvlNode<TKey, TValue> node)
    {
        int leftHeight = node.Left?.Height ?? 0;
        int rightHeight = node.Right?.Height ?? 0;
        return leftHeight - rightHeight;
    }
    
    private bool IsBalancedUpToRoot(AvlNode<TKey, TValue> startNode)
    {
        var current = startNode;
        while (current != null)
        {
            if (Math.Abs(GetBalance(current)) >= 2)
                return false;
            current = current.Parent;
        }
        return true;
    }
}