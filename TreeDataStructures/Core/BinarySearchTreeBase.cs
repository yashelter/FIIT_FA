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

            switch (cmp)
            {
                case < 0:
                    current = current.Left;
                    
                    break;
                case > 0:
                    current = current.Right;
                    
                    break;
                default:
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
        TNode? rebalanceParent;
        TNode? replacement;

        if (node.Left == null || node.Right == null)
        {
            replacement = node.Left ?? node.Right;
            rebalanceParent = node.Parent;

            Transplant(node, replacement);
            OnNodeRemoved(rebalanceParent, replacement);

            return;
        }

        var successor = Minimum(node.Right);
        var successorOldParent = successor.Parent;
        var successorOldRight = successor.Right;

        if (successorOldParent != node)
        {
            Transplant(successor, successorOldRight);

            successor.Right = node.Right;
            successor.Right!.Parent = successor;
        }

        Transplant(node, successor);

        successor.Left = node.Left;
        successor.Left!.Parent = successor;

        replacement = successor;
        rebalanceParent = successorOldParent == node ? successor : successorOldParent;

        OnNodeRemoved(rebalanceParent, replacement);
    }

    protected void RotateLeft(TNode x)
    {
        var y = x.Right;
        if (y == null)
        {
            throw new InvalidOperationException("Left rotation requires a right child.");
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
            throw new InvalidOperationException("Right rotation requires a left child.");
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
        if (x.Right?.Left == null)
        {
            throw new InvalidOperationException("Double left rotation requires right child with left subtree.");
        }

        RotateRight(x.Right);
        RotateLeft(x);
    }

    protected void RotateDoubleRight(TNode y)
    {
        if (y.Left?.Right == null)
        {
            throw new InvalidOperationException("Double right rotation requires left child with right subtree.");
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

    /// <summary>
    /// Внутренний класс-итератор.
    /// Реализует паттерн Iterator вручную, без yield return (ban).
    /// </summary>
    private abstract class TreeIterator :
        IEnumerable<TreeEntry<TKey, TValue>>,
        IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TNode? _root;
        private Stack<Frame> _stack = new();

        private TreeEntry<TKey, TValue> _current;
        private bool _hasCurrent;

        protected readonly struct Frame(TNode node, int depth, bool emit)
        {
            public TNode Node { get; } = node;
            public int Depth { get; } = depth;
            public bool Emit { get; } = emit;
        }

        protected TreeIterator(TNode? root)
        {
            _root = root;
            
            Reset();
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => Create(_root);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TreeEntry<TKey, TValue> Current =>
            _hasCurrent ? _current : throw new InvalidOperationException("Enumerator is not positioned on an element.");

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            while (_stack.Count > 0)
            {
                var frame = _stack.Pop();

                if (frame.Emit)
                {
                    _current = new TreeEntry<TKey, TValue>(frame.Node.Key, frame.Node.Value, frame.Depth);
                    _hasCurrent = true;

                    return true;
                }

                Expand(frame.Node, frame.Depth);
            }

            _hasCurrent = false;

            return false;
        }

        public void Reset()
        {
            _stack = new Stack<Frame>();
            _hasCurrent = false;

            PushNode(_root, 0);
        }

        public void Dispose()
        {
        }

        protected abstract TreeIterator Create(TNode? root);

        protected abstract void Expand(TNode node, int depth);

        protected void PushNode(TNode? node, int depth)
        {
            if (node != null)
            {
                _stack.Push(new Frame(node, depth, emit: false));
            }
        }

        protected void PushEmit(TNode node, int depth) =>
            _stack.Push(new Frame(node, depth, emit: true));
    }

    private sealed class InOrderIterator(TNode? root) : TreeIterator(root)
    {
        protected override TreeIterator Create(TNode? root) => new InOrderIterator(root);

        protected override void Expand(TNode node, int depth)
        {
            var nextDepth = depth + 1;

            PushNode(node.Right, nextDepth);
            PushEmit(node, depth);
            PushNode(node.Left, nextDepth);
        }
    }

    private sealed class PreOrderIterator(TNode? root) : TreeIterator(root)
    {
        protected override TreeIterator Create(TNode? root) => new PreOrderIterator(root);

        protected override void Expand(TNode node, int depth)
        {
            var nextDepth = depth + 1;

            PushNode(node.Right, nextDepth);
            PushNode(node.Left, nextDepth);
            PushEmit(node, depth);
        }
    }

    private sealed class PostOrderIterator(TNode? root) : TreeIterator(root)
    {
        protected override TreeIterator Create(TNode? root) => new PostOrderIterator(root);

        protected override void Expand(TNode node, int depth)
        {
            var nextDepth = depth + 1;

            PushEmit(node, depth);
            PushNode(node.Right, nextDepth);
            PushNode(node.Left, nextDepth);
        }
    }

    private sealed class InOrderReverseIterator(TNode? root) : TreeIterator(root)
    {
        protected override TreeIterator Create(TNode? root) => new InOrderReverseIterator(root);

        protected override void Expand(TNode node, int depth)
        {
            var nextDepth = depth + 1;

            PushNode(node.Left, nextDepth);
            PushEmit(node, depth);
            PushNode(node.Right, nextDepth);
        }
    }

    private sealed class PreOrderReverseIterator(TNode? root) : TreeIterator(root)
    {
        protected override TreeIterator Create(TNode? root) => new PreOrderReverseIterator(root);

        protected override void Expand(TNode node, int depth)
        {
            var nextDepth = depth + 1;

            PushEmit(node, depth);
            PushNode(node.Left, nextDepth);
            PushNode(node.Right, nextDepth);
        }
    }

    private class PostOrderReverseIterator(TNode? root) : TreeIterator(root)
    {
        protected override TreeIterator Create(TNode? root) => new PostOrderReverseIterator(root);

        protected override void Expand(TNode node, int depth)
        {
            var nextDepth = depth + 1;

            PushNode(node.Left, nextDepth);
            PushNode(node.Right, nextDepth);
            PushEmit(node, depth);
        }
    }

    private IEnumerable<TreeEntry<TKey, TValue>> InOrderTraversal(TNode? node) =>
        new InOrderIterator(node);

    private IEnumerable<TreeEntry<TKey, TValue>> PreOrderTraversal(TNode? node) =>
        new PreOrderIterator(node);

    private IEnumerable<TreeEntry<TKey, TValue>> PostOrderTraversal(TNode? node) =>
        new PostOrderIterator(node);

    private IEnumerable<TreeEntry<TKey, TValue>> InOrderReverseTraversal(TNode? node) =>
        new InOrderReverseIterator(node);

    private IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverseTraversal(TNode? node) =>
        new PreOrderReverseIterator(node);

    private IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverseTraversal(TNode? node) =>
        new PostOrderReverseIterator(node);

    #endregion
}