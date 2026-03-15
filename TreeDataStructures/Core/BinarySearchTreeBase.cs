using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null) 
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => InOrder().Select(e => e.Key).ToList();
    public ICollection<TValue> Values => InOrder().Select(e => e.Value).ToList();
    
    
    public virtual void Add(TKey key, TValue value)
    {
        if (this.Root is null)
        {
            this.Root = CreateNode(key, value);
            this.Count++;
            OnNodeAdded(this.Root);
            return;
        }

        TNode? current = this.Root;
        while (current is not null)
        {
            int comparison = Comparer.Compare(key, current.Key);
            
            if (comparison < 0)
            {
                if (current.Left is null)
                {
                    current.Left = CreateNode(key, value);
                    current.Left.Parent = current;
                    this.Count++;
                    OnNodeAdded(current.Left);
                    return;
                }
                current = current.Left;
            }

            else if (comparison > 0)
            {
                if (current.Right is null)
                {
                    current.Right = CreateNode(key, value);
                    current.Right.Parent = current;
                    this.Count++;
                    OnNodeAdded(current.Right);
                    return;
                }
                current = current.Right;
            }

            else
            {
                current.Value = value;
                return;
            }
        }
    }

    
    public virtual bool Remove(TKey key)
    {
        TNode? node = FindNode(key);
        if (node == null) { return false; }

        RemoveNode(node);
        this.Count--;
        return true;
    }
    
    
    protected virtual void RemoveNode(TNode node)
    {        
        if (node.Left is null)
        {
            Transplant(node, node.Right);
            OnNodeRemoved(node, node.Right);
        }
        else if (node.Right is null)
        {
            Transplant(node, node.Left);
            OnNodeRemoved(node, node.Left);
        }
        else
        {
            TNode replacement = FindMinNode(node.Right);
            
            if (replacement.Parent != node)
            {
                Transplant(replacement, replacement.Right);
                replacement.Right = node.Right;
                replacement.Right.Parent = replacement;
            }
            
            Transplant(node, replacement);
            replacement.Left = node.Left;
            replacement.Left.Parent = replacement;
            
            OnNodeRemoved(node, replacement);
        }
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null;
    
    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }

    
    #region Hooks
    
    /// <summary>
    /// Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode) { }
    
    /// <summary>
    /// Вызывается после удаления. 
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { }
    
    #endregion
    
    
    #region Helpers
    protected abstract TNode CreateNode(TKey key, TValue value);
    
    
    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root;
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) { return current; }
            current = cmp < 0 ? current.Left : current.Right;
        }
        return null;
    }

    protected TNode FindMinNode(TNode node)
    {
        TNode current = node;
        while (current.Left is not null)
        {
            current = current.Left;
        }
        return current;
    }

    protected void RotateLeft(TNode x)
    {
        if (x is null || x.Right is null) return;

        TNode y = x.Right;

        x.Right = y.Left;
        if (y.Left is not null)
        {
            y.Left.Parent = x;
        }

        y.Parent = x.Parent;
        if (x.Parent is null)
        {
            this.Root = y;
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
        if (y is null || y.Left is null) return;

        TNode x = y.Left;

        y.Left = x.Right;
        if (x.Right is not null)
        {
            x.Right.Parent = y;
        }

        x.Parent = y.Parent;
        if (y.Parent is null)
        {
            this.Root = x;
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
        RotateRight(x.Right!);
        RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        RotateLeft(y.Left!);
        RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        RotateLeft(x);
        RotateLeft(x.Parent!);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        RotateRight(y);
        RotateRight(y.Parent!);
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
    
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder);
    public IEnumerable<TreeEntry<TKey, TValue>>  InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse);
    public IEnumerable<TreeEntry<TKey, TValue>>  PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse);
    
    /// <summary>
    /// Внутренний класс-итератор. 
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private struct TreeIterator : 
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TNode? root;
        private readonly TraversalStrategy strategy;
        private Stack<(TNode node, int depth)> stack = new();
        private readonly int baseDepth;

        private bool started = false;
        private bool finished = false;
        private TreeEntry<TKey, TValue> current;
        
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        
        public TreeEntry<TKey, TValue> Current => current;
        object IEnumerator.Current => Current;
        
        public TreeIterator(TNode? root, TraversalStrategy strategy, int baseDepth = 0)
        {
            this.root = root;
            this.strategy = strategy;
            this.baseDepth = baseDepth;
        }
        public bool MoveNext()
        {
            if (finished) return false;
            
            switch (strategy)
            {
                case TraversalStrategy.PreOrder:
                    return MoveNextPreOrder();
                case TraversalStrategy.InOrder:
                    return MoveNextInOrder();
                case TraversalStrategy.PostOrder:
                    return MoveNextPostOrder();
                case TraversalStrategy.PreOrderReverse:
                    return MoveNextPostOrder(flipped: true);
                case TraversalStrategy.InOrderReverse:
                    return MoveNextInOrder(flipped: true);
                case TraversalStrategy.PostOrderReverse:
                    return MoveNextPreOrder(flipped: true);
            }
            
            throw new NotImplementedException();
        }

        public bool MoveNextInOrder(bool flipped = false)
        {
            if (!started)
            {
                started = true;
                if (flipped)
                    PushRightChain(root, baseDepth);
                else 
                    PushLeftChain(root, baseDepth);
            }

            if (stack.Count == 0)
            {
                finished = true;
                return false;
            }

            (TNode node, int depth) = stack.Pop();
            current = new(node.Key, node.Value, depth);

            if (flipped)
            {    
                if (node.Left is not null) PushRightChain(node.Left, depth);
            }
            else
            {
                if (node.Right is not null) PushLeftChain(node.Right, depth);
            }
            return true;
        }

        public void PushLeftChain(TNode? node, int depth)
        {
            while (node is not null)
            {
                stack.Push((node, depth));
                node = node.Left;
                depth++;
            }
        }

        public void PushRightChain(TNode? node, int depth)
        {
            while (node is not null)
            {
                stack.Push((node, depth));
                node = node.Right;
                depth++;
            }
        }

        public bool MoveNextPreOrder(bool flipped = false)
        {
            if (!started)
            {
                started = true;
                if (root is not null) stack.Push((root, baseDepth));
            }

            if (stack.Count == 0)
            {
                finished = true;
                return false;
            }

            (TNode node, int depth) = stack.Pop();
            current = new(node.Key, node.Value, depth);
            
            if (flipped)
            {
                if (node.Left is not null) stack.Push((node.Left, depth + 1));
                if (node.Right is not null) stack.Push((node.Right, depth + 1));
            }
            else
            {
                if (node.Right is not null) stack.Push((node.Right, depth + 1));
                if (node.Left is not null) stack.Push((node.Left, depth + 1));
            }

            return true;
        }
        

        public bool MoveNextPostOrder(bool flipped = false)
        {
            if (!started)
            {
                started = true;
                if (flipped)
                {
                    if (root is not null) PushRightLeftChain(root, baseDepth);
                }
                else
                {
                    if (root is not null) PushLeftRightChain(root, baseDepth);
                }
            }

            if (stack.Count == 0)
            {
                finished = true;
                return false;
            }
            
            (TNode node, int depth) = stack.Pop();
            current = new(node.Key, node.Value, depth);

            if (flipped)
            {
                if (node.Parent is not null && node.IsRightChild) PushRightLeftChain(node.Parent.Left, depth);
            }
            else
            {
                if (node.Parent is not null && node.IsLeftChild) PushLeftRightChain(node.Parent.Right, depth);
            }

            return true;
            
        }

        public void PushLeftRightChain(TNode? node, int depth)
        {
            while (node is not null)
            {
                stack.Push((node, depth));
                if (node.Left is not null) node = node.Left;
                else if (node.Right is not null) node = node.Right;
                else node = null;
                depth++;
            }
        }

        public void PushRightLeftChain(TNode? node, int depth)
        {
             while (node is not null)
            {
                stack.Push((node, depth));
                if (node.Right is not null) node = node.Right;
                else if (node.Left is not null) node = node.Left;
                else node = null;
                depth++;
            }
        }

        public void Reset()
        {
            stack.Clear();
            started = false;
            finished = false;
            current = default;
        }

        
        public void Dispose()
        {
            stack.Clear();
            finished = true;
            current = default;
        }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return InOrder().Select(e => new KeyValuePair<TKey, TValue>(e.Key, e.Value)).GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) 
            throw new ArgumentNullException(nameof(array));
        
        if (arrayIndex < 0) 
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        
        if (array.Length - arrayIndex < Count) 
            throw new ArgumentException("Not enough space in array");

        foreach (var entry in InOrder())
        {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}