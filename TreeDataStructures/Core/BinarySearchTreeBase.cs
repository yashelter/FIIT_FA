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
        if (x is null || x.Right is null) return;
        TNode rightChild = x.Right;
        RotateRight(rightChild);
        RotateLeft(x);
    }

    protected void RotateBigRight(TNode y)
    {
        if (y is null || y.Left is null) return;
        TNode leftChild = y.Left;
        RotateLeft(leftChild);
        RotateRight(y);
    }

    protected void RotateDoubleLeft(TNode x)
    {
        if (x is null || x.Right is null) return;
        RotateLeft(x);
        if (x.Parent is not null)
        {
            RotateLeft(x.Parent);
        }
    }

    protected void RotateDoubleRight(TNode y)
    {
        if (y is null || y.Left is null) return;
        RotateRight(y);
        if (y.Parent is not null)
        {
            RotateRight(y.Parent);
        }
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
        private TNode? currentNode;
        private TNode? lastVisited;
        private int currentDepth; 
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
                if (root is null)
                {
                    finished = true;
                    return false;
                }
                
                currentNode = root;
                currentDepth = baseDepth;
                if (flipped)
                {
                    while (currentNode.Right is not null)
                    {
                        currentNode = currentNode.Right;
                        currentDepth++;
                    }
                }
                else
                {
                    while (currentNode.Left is not null)
                    {
                        currentNode = currentNode.Left;
                        currentDepth++;
                    }
                }
                return YieldCurrent();
            }

            if (finished || currentNode is null) return false;

            TNode? next;
            if (flipped)
            {
                if (currentNode.Left is not null)
                {
                    next = currentNode.Left;
                    currentDepth++;
                    while (next.Right is not null)
                    {
                        next = next.Right;
                        currentDepth++;
                    }
                    currentNode = next;
                    return YieldCurrent();
                }
                
                while (currentNode.Parent is not null && currentNode.IsLeftChild)
                {
                    currentNode = currentNode.Parent;
                    currentDepth--;
                }
                
                if (currentNode.Parent is null)
                {
                    finished = true;
                    return false;
                }
                
                currentNode = currentNode.Parent;
                currentDepth--;
            }
            else
            {
                if (currentNode.Right is not null)
                {
                    next = currentNode.Right;
                    currentDepth++;
                    while (next.Left is not null)
                    {
                        next = next.Left;
                        currentDepth++;
                    }
                    currentNode = next;
                    return YieldCurrent();
                }
                
                while (currentNode.Parent is not null && currentNode.IsRightChild)
                {
                    currentNode = currentNode.Parent;
                    currentDepth--;
                }
                
                if (currentNode.Parent is null)
                {
                    finished = true;
                    return false;
                }
                
                currentNode = currentNode.Parent;
                currentDepth--;
            }
            
            return currentNode is not null && YieldCurrent();
        }
        

        public bool MoveNextPreOrder(bool flipped = false)
        {
            if (!started)
            {
                started = true;
                if (root is null)
                {
                    finished = true;
                    return false;
                }
                currentNode = root;
                currentDepth = baseDepth;
                return YieldCurrent();
            }

            if (finished || currentNode is null) return false;

            TNode? firstChild = flipped ? currentNode.Right : currentNode.Left;
            TNode? secondChild = flipped ? currentNode.Left : currentNode.Right;
            
            if (firstChild is not null)
            {
                currentNode = firstChild;
                currentDepth++;
                return YieldCurrent();
            }
            
            if (secondChild is not null)
            {
                currentNode = secondChild;
                currentDepth++;
                return YieldCurrent();
            }
            
            while (currentNode?.Parent is not null)
            {
                TNode parent = currentNode.Parent;
                TNode? pFirst = flipped ? parent.Right : parent.Left;
                TNode? pSecond = flipped ? parent.Left : parent.Right;
                
                if (currentNode == pFirst && pSecond is not null)
                {
                    currentNode = pSecond;
                    currentDepth++;
                    return YieldCurrent();
                }
                
                currentNode = parent;
                currentDepth--;
            }
            
            finished = true;
            return false;
        }


        public bool MoveNextPostOrder(bool flipped = false)
        {
            if (!started)
            {
                started = true;
                if (root is null) 
                {
                    finished = true;
                    return false;
                }
                
                currentNode = root;
                currentDepth = baseDepth;
                while (true)
                {
                    TNode? first = flipped ? currentNode.Right : currentNode.Left;
                    TNode? second = flipped ? currentNode.Left : currentNode.Right;
                    
                    if (first is not null)
                    {
                        currentNode = first;
                        currentDepth++;
                    }
                    else if (second is not null)
                    {
                        currentNode = second;
                        currentDepth++;
                    }
                    else break;
                }
                lastVisited = null;
                return YieldCurrentWithMark();
            }

            if (finished || currentNode is null) return false;

            while (currentNode?.Parent is not null)
            {
                TNode parent = currentNode.Parent;
                TNode? firstChild = flipped ? parent.Right : parent.Left;
                TNode? secondChild = flipped ? parent.Left : parent.Right;
                
                if (currentNode == firstChild && secondChild is not null && secondChild != lastVisited)
                {
                    currentNode = secondChild;
                    currentDepth++;
                    while (true)
                    {
                        TNode? f = flipped ? currentNode.Right : currentNode.Left;
                        TNode? s = flipped ? currentNode.Left : currentNode.Right;
                        if (f is not null)
                        {
                            currentNode = f;
                            currentDepth++;
                        }
                        else if (s is not null)
                        {
                            currentNode = s;
                            currentDepth++;
                        }
                        else break;
                    }
                    return YieldCurrentWithMark();
                }
                
                currentNode = parent;
                currentDepth--;
                return YieldCurrentWithMark();
            }
            
            finished = true;
            return false;
        }

        private bool YieldCurrentWithMark()
        {
            if (currentNode is null) return false;
            current = new TreeEntry<TKey, TValue>(currentNode.Key, currentNode.Value, currentDepth);
            lastVisited = currentNode;
            return true;
        }

        private bool YieldCurrent()
        {
            if (currentNode is null) return false;
            current = new TreeEntry<TKey, TValue>(currentNode.Key, currentNode.Value, currentDepth);
            return true;
        }
        private static int CalculateDepth(TNode? node, int baseDepth = 0)
        {
            int depth = baseDepth;
            while (node?.Parent is not null)
            {
                depth++;
                node = node.Parent;
            }
            return depth;
        }
        public void Reset()
        {
            currentNode = null;
            lastVisited = null;
            currentDepth = baseDepth;
            started = false;
            finished = false;
            current = default;
        }

        
        public void Dispose()
        {
            currentNode = null;
            lastVisited = null;
            finished = true;
            started = false;
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