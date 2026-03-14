using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null)
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    #region Fields
    
    protected TNode? Root;

    public int Count { get; protected set; }

    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default;

    #endregion

    #region Public methods
    
    public virtual void Add(TKey key, TValue value)
    {
        var node = CreateNode(key, value);

        if (Root == null)
        {
            Root = node;
            Count++;

            OnNodeAdded(node);

            return;
        }

        TNode? parent = null, current = Root;
        while (current != null)
        {
            parent = current;
            
            var cmp = Comparer.Compare(key, current.Key);
            
            if (cmp < 0)
            {
                current = current.Left;
            }
            else if (cmp > 0)
            {
                current = current.Right;
            }
            else
            {
                current.Value = value;
                
                return;
            }
        }

        node.Parent = parent;
        if (Comparer.Compare(key, parent!.Key) < 0)
        {
            parent.Left = node;
        }
        else
        {
            parent.Right = node;
        }

        Count++;
        
        OnNodeAdded(node);
    }
    
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    
    public virtual bool Remove(TKey key)
    {
        var node = FindNode(key);
        
        if (node == null)
        {
            return false;
        }

        RemoveNode(node); 
        
        Count--;
        
        return true;
    }
    
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    
    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;

    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        
        if (node != null)
        {
            value = node.Value;
           
            return true;
        }

        value = default;
        
        return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);
        ArgumentOutOfRangeException.ThrowIfLessThan(array.Length - arrayIndex, Count);

        var index = arrayIndex;
        foreach (var pair in this)
        {
            array[index++] = pair;
        }
    }

    public void Clear()
    {
        Root = null;
        Count = 0;
    }

    public IEnumerable<TreeEntry<TKey, TValue>> InOrder() => InOrderTraversal(Root);

    public IEnumerable<TreeEntry<TKey, TValue>> PreOrder() => PreOrderTraversal(Root);

    public IEnumerable<TreeEntry<TKey, TValue>> PostOrder() => PostOrderTraversal(Root);

    public IEnumerable<TreeEntry<TKey, TValue>> InOrderReverse() => InOrderReverseTraversal(Root);

    public IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverse() => PreOrderReverseTraversal(Root);

    public IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverse() => PostOrderReverseTraversal(Root);
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return InOrder().Select(entry => new KeyValuePair<TKey, TValue>(entry.Key, entry.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => InOrder().Select(x => x.Key).ToList();
    
    public ICollection<TValue> Values => InOrder().Select(x => x.Value).ToList();
    
    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }
    
    #endregion
    
    #region Hooks

    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode)
    {
    }

    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child)
    {
    }

    #endregion
    
    #region Helpers

    protected abstract TNode CreateNode(TKey key, TValue value);
    
    protected TNode? FindNode(TKey key)
    {
        var current = Root;
        
        while (current != null)
        {
            var cmp = Comparer.Compare(key, current.Key);
            
            if (cmp == 0)
            {
                return current;
            }

            current = cmp < 0 ? current.Left : current.Right;
        }

        return null;
    }

    protected TNode Minimum(TNode node)
    {
        var current = node;
        
        while (current.Left != null)
        {
            current = current.Left;
        }

        return current;
    }

    protected virtual void RemoveNode(TNode node)
    {
        var parentBefore = node.Parent;

        if (node.Left == null)
        {
            Transplant(node, node.Right);
            
            OnNodeRemoved(parentBefore, node.Right);
            
            return;
        }

        if (node.Right == null)
        {
            Transplant(node, node.Left);
            
            OnNodeRemoved(parentBefore, node.Left);
            
            return;
        }

        var successor = Minimum(node.Right);

        if (successor.Parent != node)
        {
            Transplant(successor, successor.Right);
            
            successor.Right = node.Right;
            successor.Right?.Parent = successor;
        }

        Transplant(node, successor);
        
        successor.Left = node.Left;
        successor.Left?.Parent = successor;

        OnNodeRemoved(parentBefore, successor);
    }
    
    protected void RotateLeft(TNode x)
    {
        var y = x.Right;
        if (y == null)
        {
            return;
        }

        x.Right = y.Left;
        y.Left?.Parent = x;

        y.Parent = x.Parent;
        if (x.Parent == null)
        {
            Root = y;
        }
        else if (x.IsLeftChild)
        {
            x.Parent.Left = y;
        }
        else
        {
            x.Parent.Right = y;
        }

        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        var x = y.Left;
        if (x == null)
        {
            return;
        }

        y.Left = x.Right;
        x.Right?.Parent = y;

        x.Parent = y.Parent;
        if (y.Parent == null)
        {
            Root = x;
        }
        else if (y.IsLeftChild)
        {
            y.Parent.Left = x;
        }
        else
        {
            y.Parent.Right = x;
        }

        x.Right = y;
        y.Parent = x;
    }

    protected void RotateBigLeft(TNode x)
    {
        RotateDoubleLeft(x);
    }

    protected void RotateBigRight(TNode y)
    {
        RotateDoubleRight(y);
    }

    protected void RotateDoubleLeft(TNode x)
    {
        if (x.Right == null)
        {
            return;
        }

        RotateRight(x.Right);
        RotateLeft(x);
    }

    protected void RotateDoubleRight(TNode y)
    {
        if (y.Left == null)
        {
            return;
        }

        RotateLeft(y.Left);
        RotateRight(y);
    }

    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
        {
            Root = v;
        }
        else if (u.IsLeftChild)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }

        v?.Parent = u.Parent;
    }

    #endregion

    #region Iterators
    
    private enum TraversalStrategy
    {
        InOrder,
        PreOrder,
        PostOrder,
        InOrderReverse,
        PreOrderReverse,
        PostOrderReverse
    }
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator :
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TraversalStrategy _strategy;
        private readonly TreeEntry<TKey, TValue>[] _items;
        private int _index;

        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _strategy = strategy;
            _items = BuildItems(root, strategy);
            _index = -1;
        }

        private static TreeEntry<TKey, TValue>[] BuildItems(TNode? root, TraversalStrategy strategy)
        {
            var list = new List<TreeEntry<TKey, TValue>>();

            switch (strategy)
            {
                case TraversalStrategy.InOrder:
                    BuildInOrder(root, 0, list);
                    
                    break;
                case TraversalStrategy.PreOrder:
                    BuildPreOrder(root, 0, list);
                    
                    break;
                case TraversalStrategy.PostOrder:
                    BuildPostOrder(root, 0, list);
                    
                    break;
                case TraversalStrategy.InOrderReverse:
                    BuildInOrder(root, 0, list);
                    
                    list.Reverse();
                    
                    break;
                case TraversalStrategy.PreOrderReverse:
                    BuildPreOrder(root, 0, list);
                    
                    list.Reverse();
                    
                    break;
                case TraversalStrategy.PostOrderReverse:
                    BuildPostOrder(root, 0, list);
                    
                    list.Reverse();
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }

            return list.ToArray();
        }

        private static void BuildInOrder(TNode? node, int depth, List<TreeEntry<TKey, TValue>> list)
        {
            if (node == null)
            {
                return;
            }

            BuildInOrder(node.Left, depth + 1, list);
            list.Add(new TreeEntry<TKey, TValue>(node.Key, node.Value, depth));
            BuildInOrder(node.Right, depth + 1, list);
        }

        private static void BuildPreOrder(TNode? node, int depth, List<TreeEntry<TKey, TValue>> list)
        {
            if (node == null)
            {
                return;
            }

            list.Add(new TreeEntry<TKey, TValue>(node.Key, node.Value, depth));
            BuildPreOrder(node.Left, depth + 1, list);
            BuildPreOrder(node.Right, depth + 1, list);
        }

        private static void BuildPostOrder(TNode? node, int depth, List<TreeEntry<TKey, TValue>> list)
        {
            if (node == null)
            {
                return;
            }

            BuildPostOrder(node.Left, depth + 1, list);
            BuildPostOrder(node.Right, depth + 1, list);
            list.Add(new TreeEntry<TKey, TValue>(node.Key, node.Value, depth));
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        public TreeEntry<TKey, TValue> Current => _items[_index];
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            var next = _index + 1;

            if (next >= _items.Length)
            {
                return false;
            }

            _index = next;

            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
        }
    }

    private IEnumerable<TreeEntry<TKey, TValue>> InOrderTraversal(TNode? node) =>
        new TreeIterator(node, TraversalStrategy.InOrder);
    
    private IEnumerable<TreeEntry<TKey, TValue>> PreOrderTraversal(TNode? node) =>
        new TreeIterator(node, TraversalStrategy.PreOrder);
    
    private IEnumerable<TreeEntry<TKey, TValue>> PostOrderTraversal(TNode? node) =>
        new TreeIterator(node, TraversalStrategy.PostOrder);
    
    private IEnumerable<TreeEntry<TKey, TValue>> InOrderReverseTraversal(TNode? node) =>
        new TreeIterator(node, TraversalStrategy.InOrderReverse);
    
    private IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverseTraversal(TNode? node) =>
        new TreeIterator(node, TraversalStrategy.PreOrderReverse);
    
    private IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverseTraversal(TNode? node) =>
        new TreeIterator(node, TraversalStrategy.PostOrderReverse);
    
    #endregion
}