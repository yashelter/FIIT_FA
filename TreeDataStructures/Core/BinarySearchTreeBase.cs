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
        TNode z = CreateNode(key, value);
        TNode? y = null;
        TNode? x = Root;

        while (x != null)
        {
            y = x;
            int cmp = Comparer.Compare(z.Key, x.Key);
            if (cmp == 0)
            {
                x.Value = value;
                return;
            }
            else if (cmp < 0)
            {
                x = x.Left;
            }
            else
            {
                x = x.Right;
            }
        }

        z.Parent = y;
        if (y == null)
        {
            Root = z;
        }
        else if (Comparer.Compare(z.Key, y.Key) < 0)
        {
            y.Left = z;
        }
        else
        {
            y.Right = z;
        }

        Count++;
        OnNodeAdded(z);
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
        TNode? parentToNotify;
        TNode? childToNotify;

        if (node.Left == null)
        {
            parentToNotify = node.Parent;
            childToNotify = node.Right;
            Transplant(node, node.Right);
            OnNodeRemoved(parentToNotify, childToNotify);
        }
        else if (node.Right == null)
        {
            parentToNotify = node.Parent;
            childToNotify = node.Left;
            Transplant(node, node.Left);
            OnNodeRemoved(parentToNotify, childToNotify);
        }
        else
        {
            TNode y = node.Right;
            while (y.Left != null)
            {
                y = y.Left;
            }

            if (y.Parent != node)
            {
                parentToNotify = y.Parent;
                childToNotify = y.Right;

                Transplant(y, y.Right);
                y.Right = node.Right;
                y.Right.Parent = y;
            }
            else
            {
                parentToNotify = y;
                childToNotify = y.Right;
            }

            Transplant(node, y);
            y.Left = node.Left;
            y.Left.Parent = y;

            OnNodeRemoved(parentToNotify, childToNotify);
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

    protected void RotateLeft(TNode x)
    {
        TNode? y = x.Right;
        if (y == null) return;

        x.Right = y.Left;
        if (y.Left != null)
        {
            y.Left.Parent = x;
        }

        y.Parent = x.Parent;
        if (x.Parent == null)
        {
            Root = y;
        }
        else if (x == x.Parent.Left)
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
        TNode? x = y.Left;
        if (x == null) return;

        y.Left = x.Right;
        if (x.Right != null)
        {
            x.Right.Parent = y;
        }

        x.Parent = y.Parent;
        if (y.Parent == null)
        {
            Root = x;
        }
        else if (y == y.Parent.Right)
        {
            y.Parent.Right = x;
        }
        else
        {
            y.Parent.Left = x;
        }

        x.Right = y;
        y.Parent = x;
    }
    
    protected void RotateBigLeft(TNode x)
    {
        TNode? p = x.Right;
        if (p == null) return;
        RotateLeft(x);
        RotateLeft(p);
    }
    
    protected void RotateBigRight(TNode y)
    {
        TNode? p = y.Left;
        if (p == null) return;
        RotateRight(y);
        RotateRight(p);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        if (x.Right == null) return;
        RotateRight(x.Right);
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        if (y.Left == null) return;
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
        private readonly TraversalStrategy _strategy;
        private readonly TNode? _root;
        private Stack<TNode>? _stack;
        private TNode? _current;
        private TNode? _lastVisited;

        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _root = root;
            _strategy = strategy;
            _stack = new Stack<TNode>();
            _current = null;
            _lastVisited = null;

            Initialize();
        }

        private void Initialize()
        {
             if (_root == null) return;

             switch (_strategy)
             {
                 case TraversalStrategy.PreOrder:
                 case TraversalStrategy.PreOrderReverse:
                     _stack!.Push(_root);
                     break;

                 case TraversalStrategy.InOrder:
                     PushLeftPath(_root);
                     break;
                 case TraversalStrategy.InOrderReverse:
                     PushRightPath(_root);
                     break;

                 case TraversalStrategy.PostOrder:
                 case TraversalStrategy.PostOrderReverse:
                     _stack!.Push(_root);
                     break;
             }
        }
        
        private void PushLeftPath(TNode node)
        {
            TNode? curr = node;
            while (curr != null)
            {
                _stack!.Push(curr);
                curr = curr.Left;
            }
        }

        private void PushRightPath(TNode node)
        {
            TNode? curr = node;
            while (curr != null)
            {
                _stack!.Push(curr);
                curr = curr.Right;
            }
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        
        public TreeEntry<TKey, TValue> Current
        {
            get
            {
                if (_current == null) throw new InvalidOperationException();
                int depth = 0;
                var p = _current.Parent;
                while (p != null) { depth++; p = p.Parent; }
                return new TreeEntry<TKey, TValue>(_current.Key, _current.Value, depth);
            }
        }
        
        object IEnumerator.Current => Current;
        
        public bool MoveNext()
        {
            if (_stack == null || _stack.Count == 0) return false;

            switch (_strategy)
            {
                case TraversalStrategy.PreOrder:
                    return MoveNextPreOrder();
                case TraversalStrategy.PreOrderReverse:
                    return MoveNextPostOrderReverse();
                case TraversalStrategy.InOrder:
                    return MoveNextInOrder();
                case TraversalStrategy.InOrderReverse:
                    return MoveNextInOrderReverse();
                case TraversalStrategy.PostOrder:
                    return MoveNextPostOrder();
                case TraversalStrategy.PostOrderReverse:
                    return MoveNextPreOrderReverse();
                default:
                    return false;
            }
        }

        private bool MoveNextPreOrder()
        {
            if (_stack!.Count == 0) return false;
            _current = _stack.Pop();

            if (_current.Right != null) _stack.Push(_current.Right);
            if (_current.Left != null) _stack.Push(_current.Left);

            return true;
        }

        private bool MoveNextPreOrderReverse()
        {
            if (_stack!.Count == 0) return false;
            _current = _stack.Pop();

            if (_current.Left != null) _stack.Push(_current.Left);
            if (_current.Right != null) _stack.Push(_current.Right);

            return true;
        }

        private bool MoveNextInOrder()
        {
             if (_stack!.Count == 0) return false;
             _current = _stack.Pop();

             if (_current.Right != null)
             {
                 PushLeftPath(_current.Right);
             }
             return true;
        }
        
        private bool MoveNextInOrderReverse()
        {
             if (_stack!.Count == 0) return false;
             _current = _stack.Pop();

             if (_current.Left != null)
             {
                 PushRightPath(_current.Left);
             }
             return true;
        }

        private bool MoveNextPostOrder()
        {
             while (_stack!.Count > 0)
             {
                 var peek = _stack.Peek();
                 bool traversingDown = _lastVisited == null || (_lastVisited != peek.Left && _lastVisited != peek.Right);

                 if (traversingDown)
                 {
                     if (peek.Left != null)
                     {
                         _stack.Push(peek.Left);
                     }
                     else if (peek.Right != null)
                     {
                         _stack.Push(peek.Right);
                     }
                     else
                     {
                         _current = _stack.Pop();
                         _lastVisited = _current;
                         return true;
                     }
                 }
                 else if (_lastVisited == peek.Left)
                 {
                     if (peek.Right != null)
                     {
                         _stack.Push(peek.Right);
                     }
                     else
                     {
                         _current = _stack.Pop();
                         _lastVisited = _current;
                         return true;
                     }
                 }
                 else
                 {
                     _current = _stack.Pop();
                     _lastVisited = _current;
                     return true;
                 }
             }
             return false;
        }

        private bool MoveNextPostOrderReverse()
        {
             while (_stack!.Count > 0)
             {
                 var peek = _stack.Peek();
                 bool traversingDown = _lastVisited == null || (_lastVisited != peek.Left && _lastVisited != peek.Right);

                 if (traversingDown)
                 {
                     if (peek.Right != null)
                     {
                         _stack.Push(peek.Right);
                     }
                     else if (peek.Left != null)
                     {
                         _stack.Push(peek.Left);
                     }
                     else
                     {
                         _current = _stack.Pop();
                         _lastVisited = _current;
                         return true;
                     }
                 }
                 else if (_lastVisited == peek.Right)
                 {
                     if (peek.Left != null)
                     {
                         _stack.Push(peek.Left);
                     }
                     else
                     {
                         _current = _stack.Pop();
                         _lastVisited = _current;
                         return true;
                     }
                 }
                 else
                 {
                     _current = _stack.Pop();
                     _lastVisited = _current;
                     return true;
                 }
             }
             return false;
        }
        
        public void Reset()
        {
            _stack?.Clear();
            _current = null;
            _lastVisited = null;
            Initialize();
        }


        public void Dispose() { }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var entry in InOrder())
        {
            yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}