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

    public ICollection<TKey> Keys => InOrder().Select(e => e.Key).ToArray();
    public ICollection<TValue> Values => InOrder().Select(e => e.Value).ToArray();

    private class KeyValuePairIterator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private IEnumerator<TreeEntry<TKey, TValue>> _inner;

        public KeyValuePairIterator(IEnumerator<TreeEntry<TKey, TValue>> inner)
        {
            _inner = inner;
        }

        public KeyValuePair<TKey, TValue> Current =>
            new KeyValuePair<TKey, TValue>(_inner.Current.Key, _inner.Current.Value);

        object IEnumerator.Current => Current;

        public bool MoveNext() => _inner.MoveNext();
        public void Reset() => _inner.Reset();
        public void Dispose() => _inner.Dispose();
    }


    public virtual void Add(TKey key, TValue value)
    {

        TNode? existing = FindNode(key);
        if (existing != null)
        {
            existing.Value = value;
            return;
        }


        TNode newNode = CreateNode(key, value);

        if (Root == null)
        {
            Root = newNode;
        }
        else
        {
            TNode current = Root;
            TNode parent = null;

            while (current != null)
            {
                parent = current;

                int cmpRes1 = Comparer.Compare(key, current.Key);

                if (cmpRes1 < 0)
                {
                    current = current.Left;
                }

                else if (cmpRes1 > 0)
                {
                    current = current.Right;
                }
            }       
            int cmpRes2 = Comparer.Compare(key, parent.Key);
            if (cmpRes2 < 0)
            {
                parent.Left = newNode;
            }

            else if (cmpRes2 > 0)
            {
                parent.Right = newNode;
            }
            newNode.Parent = parent;
        }
        this.Count++;
        OnNodeAdded(newNode);
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
        if (node.Left == null && node.Right == null)
        {
            Transplant(node, null);
            OnNodeRemoved(node.Parent, null);
        }
        else if (node.Left == null && node.Right != null)
        {
            Transplant(node, node.Right);
            OnNodeRemoved(node.Parent, node.Right);
        }
        else if (node.Left != null && node.Right == null)
        {
            Transplant(node, node.Left);
            OnNodeRemoved(node.Parent, node.Left);
        }
        else
        {
            TNode current = node.Right;
            while (current.Left != null) 
            {
                current = current.Left;
            }
            if (current.Parent != node)
            {
                current.Parent.Left = current.Right;
                if (current.Right != null)
                    current.Right.Parent = current.Parent;

                current.Right = node.Right;
                node.Right.Parent = current;
            }
            current.Left = node.Left;
            node.Left.Parent = current;

            if (node.Parent == null)
            {
                current.Parent = null;
                Root = current;
            }

            else if (node.IsLeftChild) {
                node.Parent.Left = current;
                current.Parent = node.Parent;
            }
            else
            {
                node.Parent.Right = current;
                current.Parent = node.Parent;
            }
            OnNodeRemoved(current.Parent, current);
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
        TNode y = x.Right;

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
        TNode x = y.Left;

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
        else if (y.IsRightChild)
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
        TNode y = x.Right;

        RotateRight(y);
        RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        TNode x = y.Left;

        RotateLeft(x);
        RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        RotateLeft(x);
        RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        RotateRight(y);
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

    //public IEnumerable<TreeEntry<TKey, TValue>> InOrder() => InOrderTraversal(Root);

    //private IEnumerable<TreeEntry<TKey, TValue>> InOrderTraversal(TNode? node)
    //{
    //    if (node == null) { yield break; }
    //    throw new NotImplementedException();
    //}

    public IEnumerable<TreeEntry<TKey, TValue>> InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder);
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder);
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
        private readonly TNode? _root;
        private TNode? _current;
        private Stack<TNode> _stack = new Stack<TNode>();
        private Stack<TNode> _postOrderStack;
        private Stack<TNode> _postOrderReverseStack;
        private readonly TraversalStrategy _strategy; // or make it template parameter?
        
        // Конструктор
        public TreeIterator(TNode Root, TraversalStrategy strategy)
        {
            _root = Root;
            _strategy = strategy;
            _stack = new Stack<TNode>();
            _current = null;
            _postOrderStack = null;
            _postOrderReverseStack = null;
        }
        
        private void PushLeftBranch(TNode? node)
        {
            while (node != null)
            {
                _stack.Push(node);
                node = node.Left;
            }
        }

        private void PushRightBranch(TNode? node)
        {
            while (node != null)
            {
                _stack.Push(node);
                node = node.Right;
            }
        }

        private void FillPostOrderStackReverse()
        {
            Stack<TNode> tempStack = new Stack<TNode>();
            tempStack.Push(_root);

            _postOrderReverseStack = new Stack<TNode>();

            while (tempStack.Count > 0)
            {
                TNode current = tempStack.Pop();

                _postOrderReverseStack.Push(current);

                if (current.Right != null)
                {
                    tempStack.Push(current.Right);
                }
                if (current.Left != null)
                {
                    tempStack.Push(current.Left);
                }
            }
        }

        private void FillPostOrderStack()
        {
            Stack<TNode> tempStack = new Stack<TNode>();
            tempStack.Push(_root);

            _postOrderStack = new Stack<TNode>();

            while (tempStack.Count > 0)
            {
                TNode current = tempStack.Pop();

                _postOrderStack.Push(current);

                if (current.Left != null)
                {
                    tempStack.Push(current.Left);
                }
                if (current.Right != null)
                {
                    tempStack.Push(current.Right);
                }
            }
        }

        public int GetHeight(TNode? node)
        {
            if (node == null)
                return -1;

            int leftHeight = GetHeight(node.Left);
            int rightHeight = GetHeight(node.Right);

            return 1 + Math.Max(leftHeight, rightHeight);
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        
        public TreeEntry<TKey, TValue> Current
        {
            get
            {
                if (_current == null)
                {
                    throw new InvalidOperationException("Жалко");
                }

                return new TreeEntry<TKey, TValue>(
                    _current.Key,
                    _current.Value,
                    GetHeight(_current)
                    );
            }
        }
        object IEnumerator.Current => Current;
        
        
        public bool MoveNext()
        {
            // Прямой и Обртаный инфиксный
            if (_strategy == TraversalStrategy.InOrder)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    PushLeftBranch(_root);
                }
                if (_stack.Count == 0)
                {
                    return false;
                }

                _current = _stack.Pop();

                if (_current.Right != null)
                {
                    PushLeftBranch(_current.Right);
                }

                return true;
            }
            else if (_strategy == TraversalStrategy.InOrderReverse)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    PushRightBranch(_root);
                }
                if (_stack.Count == 0)
                {
                    return false;
                }

                _current = _stack.Pop();

                if (_current.Left != null)
                {
                    PushRightBranch(_current.Left);
                }
                return true;
            }
            // Прямой и обратный префиксный
            else if (_strategy == TraversalStrategy.PreOrder)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    _stack.Push(_root);
                }
                if (_stack.Count == 0)
                {
                    return false;
                }

                _current = _stack.Pop();

                if (_current.Right != null)
                {
                    _stack.Push(_current.Right);
                }
                if (_current.Left != null)
                {
                    _stack.Push(_current.Left);
                }
                return true;
            }
            else if (_strategy == TraversalStrategy.PostOrderReverse)
            {
                if (_stack.Count == 0 && _current == null)
                {
                    _stack.Push(_root);
                }
                if (_stack.Count == 0)
                {
                    return false;
                }

                _current = _stack.Pop();

                if (_current.Left != null)
                {
                    _stack.Push(_current.Left);
                }
                if (_current.Right != null)
                {
                    _stack.Push(_current.Right);
                }
                return true;
            }
            // Прямой и обратный постфиксный
            else if (_strategy == TraversalStrategy.PostOrder)
            {
                if (_postOrderStack == null)
                {
                    FillPostOrderStack();
                }
                if (_postOrderStack.Count == 0) 
                { 
                    return false;
                }

                _current = _postOrderStack.Pop();
                return true;
            }
            else if (_strategy == TraversalStrategy.PreOrderReverse)
            {
                if (_postOrderReverseStack == null)
                {
                    FillPostOrderStackReverse();
                }
                if (_postOrderReverseStack.Count == 0)
                {
                    return false;
                }

                _current = _postOrderReverseStack.Pop();
                return true;
            }
            else
            {
                throw new NotImplementedException("Неверная стратегия");
            }
        }
        
        public void Reset()
        {
            _stack?.Clear();
            _current = null;
            _postOrderStack?.Clear();
            _postOrderReverseStack?.Clear();
        }

        
        public void Dispose()
        {
            _stack?.Clear();
            _current = null;
            _postOrderStack?.Clear();
            _postOrderReverseStack?.Clear();
        }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new KeyValuePairIterator(InOrder().GetEnumerator());
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException("array");
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException("index");
        if (array.Length - arrayIndex < Count) throw new ArgumentException("Жалко");

        int i = arrayIndex;
        foreach (var k in InOrder())
        {
            array[i++] = new KeyValuePair<TKey, TValue>(k.Key, k.Value);
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}