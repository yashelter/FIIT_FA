using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null)
        {
            return (null, null);
        }

        if (Comparer.Compare(root.Key, key) <= 0)
        {
            var (leftRight, right) = Split(root.Right, key);
            root.Right = leftRight;
            if (leftRight != null)
            {
                leftRight.Parent = root;
            }
            return (root, right);
        }
        else
        {
            var (left, rightLeft) = Split(root.Left, key);
            root.Left = rightLeft;
            if (rightLeft != null)
            {
                rightLeft.Parent = root;
            }

            return (left, root);
        }
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null) return right;
        if (right == null) return left;
     
        TreapNode<TKey, TValue>? result = left; // нода для сохранения результата
        
        if (left.Priority > right.Priority)
        {
            // left становится корнем, его левое поддерево не трогаем
            // правое поддерево left сливаем с right
            left.Right = Merge(left.Right, right);
            left.Right?.Parent = left;
        }
        else
        {
            // right становится корнем, его правое поддерево не трогаем
            // left сливаем с левым поддеревом right
            right.Left = Merge(left, right.Left);
            right.Left?.Parent = right;
            
            result = right;
        }

        return result;
    }
    

    public override void Add(TKey key, TValue value)
    {
        TreapNode<TKey, TValue>? existNode = FindNode(key);
        if (existNode != null)
        {
            existNode.Value = value;
            return;
        }

        TreapNode<TKey, TValue> newNode = CreateNode(key, value);
        var (a, b) = Split(this.Root, key); //  a <= key, b > key
        TreapNode<TKey, TValue>? result = Merge(a, newNode); // Смержили с левой частью
        result = Merge(result, b);
        this.Root = result;
        this.Count++;
    }
    
    public override bool Remove(TKey key)
    {
        TreapNode<TKey, TValue>? node = FindNode(key);
        if (node == null) return false;

        TreapNode<TKey, TValue>? newNode = Merge(node.Left, node.Right);
        if (node.IsLeftChild)
        {
            node.Parent?.Left = newNode;
        }
        else if (node.IsRightChild)
        {
            node.Parent?.Right = newNode;
        }
        else
        {
            this.Root = newNode;
        }
        newNode?.Parent = node.Parent;
        this.Count--;
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        TreapNode<TKey, TValue>? newNode = new TreapNode<TKey, TValue>(key, value);
        return newNode;
    }
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
        // не нужно
    }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
        // не нужно
    }
    
}