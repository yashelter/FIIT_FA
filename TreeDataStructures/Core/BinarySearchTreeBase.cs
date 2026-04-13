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

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new List<TKey>();
            foreach (var node in InOrder())
            {
                keys.Add(node.Key);
            }

            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var values = new List<TValue>();
            foreach (var node in InOrder())
            {
                values.Add(node.Value);
            }
            
            return values;
        }
    }
    
    
    public virtual void Add(TKey key, TValue value)
    {
        TNode newNode = CreateNode(key, value);
    
        // дерево пустое
        if (Root == null)
        {
            Root = newNode;
            Count++;
            return;
        }
    
        // дерево не пустое
        TNode current = Root;
    
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key);
        
            if (cmp == 0)
            {
                current.Value = value;
                return;
            }
            else if (cmp < 0)
            {
                if (current.Left == null)
                {
                    current.Left = newNode;
                    newNode.Parent = current;
                    Count++;
                    return;
                }
                current = current.Left;
            }
            else 
            {
                if (current.Right == null)
                {
                    current.Right = newNode;
                    newNode.Parent = current;
                    Count++;
                    return;
                }
                current = current.Right;
            }
        }
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
        if (node.Left == null)
        {
            Transplant(node, node.Right);
            this.OnNodeRemoved(node.Parent, node.Right);
        }
        else if (node.Right == null)
        {
            Transplant(node, node.Left);
            this.OnNodeRemoved(node.Parent, node.Left);
        }
        else
        {
            // Ищем минимальный элемент в ПРАВОМ поддереве
            TNode? successor = node.Right;
            while (successor?.Left != null)
            {
                successor = successor.Left;
            }
            
            if (successor!.Parent != node) 
            {
                Transplant(successor!, successor.Right);
                successor.Right = node.Right;
                successor.Right!.Parent = successor;
            }
        
            Transplant(node, successor);
            successor.Left = node.Left;
            successor.Left!.Parent = successor;
        
            this.OnNodeRemoved(node.Parent, successor);
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
        if (x.Parent == null) return;
        
        TNode par = x.Parent;
        //обновляем у дедушки ребенка
        if (par.IsLeftChild) par.Parent?.Left = x;
        if (par.IsRightChild) par.Parent?.Right = x;
        //обновляем родителя у х
        x.Parent = par.Parent;
        TNode? temp = x.Left;
        
        x.Left = par;
        par.Parent = x;

        par.Right = temp;
        temp?.Parent = par;

        if (par == this.Root) this.Root = x;
    }

    protected void RotateRight(TNode y)
    {
       if(y.Parent == null) return;

       TNode par = y.Parent;
       if (par.IsLeftChild) par.Parent?.Left = y;
       if (par.IsRightChild) par.Parent?.Right = y;
       y.Parent = par.Parent;
       
       TNode? temp = y.Right;

       y.Right = par;
       par.Parent = y;

       par.Left = temp;
       temp?.Parent = par;
       
       if (par == this.Root) this.Root = y;
    }
    
    protected void RotateBigLeft(TNode x)
    {
        this.RotateRight(x);
        this.RotateLeft(x);
    }
    
    protected void RotateBigRight(TNode y)
    {
        this.RotateLeft(y);
        this.RotateRight(y);
    }
    
    protected void RotateDoubleLeft(TNode x)
    {
        if (x.Parent != null) this.RotateLeft(x.Parent);
        this.RotateLeft(x);
    }
    
    protected void RotateDoubleRight(TNode y)
    {
        if (y.Parent != null) this.RotateRight(y.Parent);
        this.RotateRight(y);
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
        // probably add something here
        private readonly TraversalStrategy _strategy; // or make it template parameter?
        
        private readonly TNode? _root;
        private TNode? _current;
        private int _depth;
        private bool _started;

        public TreeIterator(TNode? root, TraversalStrategy strategy)
        {
            _strategy = strategy;
            _root = root;
            _current = null;
            _depth = 0;
            _started = false;
        }
        
        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        public TreeEntry<TKey, TValue> Current
        {
            get
            {
                if (_current == null) throw new InvalidOperationException();
                return new TreeEntry<TKey, TValue>(_current.Key, _current.Value, _depth);
            }
        }
        object IEnumerator.Current => Current;
        
        
        public bool MoveNext()
        {
            if(_root == null) return false;

            if (!_started)
            {
                _started = true;
                _current = GetFirst();
                if (_current != null)
                {
                    _depth = GetDepth(_current);
                    return true;
                }
                return false;
            }

            _current = GetNext(_current);
            if (_current != null)
            {
                _depth = GetDepth(_current);
                return true;
            }
            
            return false;
        }
        
        //Хелперы для нодов
        private TNode? GetFirst()
        {
            return _strategy switch
            {
                TraversalStrategy.PreOrder => _root,
                TraversalStrategy.PreOrderReverse => _root, 
                TraversalStrategy.InOrder => GetLeftmost(_root),
                TraversalStrategy.InOrderReverse => GetRightmost(_root),
        
                TraversalStrategy.PostOrder => GetLeftmostLeaf(_root),
                TraversalStrategy.PostOrderReverse => GetRightmostLeaf(_root),
        
                _ => null
            };
        }

        private TNode? GetNext(TNode? node)
        {
            if (node == null) return null;
            switch (_strategy)
            {
                case TraversalStrategy.PreOrder:
                    return GetNextPreOrder(node);
                case TraversalStrategy.PreOrderReverse:
                    return GetNextPreOrderReverse(node);
                case TraversalStrategy.InOrder:
                    return GetNextInOrder(node);
                case TraversalStrategy.InOrderReverse:
                    return GetNextInOrderReverse(node);
                case TraversalStrategy.PostOrder:
                    return GetNextPostOrder(node);
                case TraversalStrategy.PostOrderReverse:
                    return GetNextPostOrderReverse(node);
                default:
                    return null;
            }
        }

        private TNode? GetNextPreOrder(TNode? node)
        {
            if (node!.Left != null) return node.Left;
            if (node.Right != null) return node.Right;

            TNode? parent = node.Parent;
            while (parent != null)
            {
                if (parent.Left == node && parent.Right != null)
                {
                    return parent.Right;
                }

                node = parent;
                parent = parent.Parent;
            }

            return null;
        }
        private TNode? GetNextPreOrderReverse(TNode? node)
        {
            if (node!.Right != null) return node.Right;
            if (node.Left != null) return node.Left;
            var parent = node.Parent;
            while (parent != null)
            {
                if (parent.Right == node && parent.Left != null)
                    return parent.Left;
                node = parent;
                parent = parent.Parent;
            }
            return null;
        }

        private TNode? GetNextInOrder(TNode? node)
        {
            if (node?.Right != null) return GetLeftmost(node.Right);
            TNode? parent = node?.Parent;
            while (parent != null && parent.Right == node)
            {
                node = parent;
                parent = parent.Parent;
            }

            return parent;
        }
        private TNode? GetNextPostOrder(TNode? node)
        {
            var parent = node!.Parent;
            if (parent == null) return null;
        
            if (parent.Left == node && parent.Right != null)
                return GetLeftmostLeaf(parent.Right);
        
            return parent;
        }
    
        private TNode? GetNextPostOrderReverse(TNode? node)
        {
            var parent = node!.Parent;
            if (parent == null) return null;
        
            if (parent.Right == node && parent.Left != null)
                return GetRightmostLeaf(parent.Left);
        
            return parent;
        }
        private TNode? GetNextInOrderReverse(TNode? node)
        {
            if (node!.Left != null) return GetRightmost(node.Left);
            
            TNode? parent = node.Parent;
            while (parent != null && parent.Left == node)
            {
                node = parent;
                parent = parent.Parent;
            }
            return parent;
        }
        private TNode? GetLeftmost(TNode? node)
        {
            while (node?.Left != null) node = node.Left;
            return node;
        }
    
        private TNode? GetRightmost(TNode? node)
        {
            while (node?.Right != null) node = node.Right;
            return node;
        }
    
        private TNode? GetLeftmostLeaf(TNode? node)
        {
            while (node != null)
            {
                if (node.Left != null) node = node.Left;
                else if (node.Right != null) node = node.Right;
                else return node;
            }
            return null;
        }
    
        private TNode? GetRightmostLeaf(TNode? node)
        {
            while (node != null)
            {
                if (node.Right != null) node = node.Right;
                else if (node.Left != null) node = node.Left;
                else return node;
            }
            return null;
        }
    
        private int GetDepth(TNode? node)
        {
            int depth = 0;
            while (node != _root)
            {
                node = node?.Parent;
                depth++;
            }
            return depth;
        }
        public void Reset()
        {
            _current = null;
            _depth = 0;
            _started = false;
        }

        
        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
    
    
    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        throw new NotImplementedException();
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public void Clear() { Root = null; Count = 0; }
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException();
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException();
        if (arrayIndex + Count > array.Length) throw new ArgumentOutOfRangeException();
        
        foreach (var item in this)
        {
            array[arrayIndex++] = item;
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}