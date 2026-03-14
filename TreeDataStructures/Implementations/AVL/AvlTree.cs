using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value) => new(key, value);

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        // После обычной BST-вставки балансируем путь к корню.
        RebalanceUpward(newNode.Parent);
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        // Не используем этот хук напрямую: балансируем в RemoveNode, где есть больше контекста.
    }

    protected override void RemoveNode(AvlNode<TKey, TValue> node)
    {
        // Точка 1: бывший родитель удаляемого узла.
        var start1 = node.Parent;

        // Точка 2: если удаляем узел с двумя детьми, может измениться поддерево
        // у старого родителя successor (он может быть не на пути к root через start1).
        AvlNode<TKey, TValue>? start2 = null;
        if (node.Left != null && node.Right != null)
        {
            var successor = Minimum(node.Right);
            if (successor.Parent != null && successor.Parent != node)
            {
                start2 = successor.Parent;
            }
        }

        base.RemoveNode(node);

        RebalanceUpward(start1);

        if (start2 != null && !ReferenceEquals(start2, start1))
        {
            RebalanceUpward(start2);
        }
    }

    private void RebalanceUpward(AvlNode<TKey, TValue>? start)
    {
        var current = start;

        while (current != null)
        {
            UpdateHeight(current);
            var balance = BalanceFactor(current);

            if (balance > 1)
            {
                // Left-Right case
                if (BalanceFactor(current.Left!) < 0)
                {
                    RotateLeft(current.Left!);
                    UpdateHeight(current.Left!);
                }

                var oldRoot = current;
                RotateRight(oldRoot);

                // oldRoot спустился вниз, его новый parent - корень локального поддерева
                UpdateHeight(oldRoot);
                if (oldRoot.Parent != null)
                {
                    UpdateHeight(oldRoot.Parent);
                    current = oldRoot.Parent.Parent;
                }
                else
                {
                    current = null;
                }
            }
            else if (balance < -1)
            {
                // Right-Left case
                if (BalanceFactor(current.Right!) > 0)
                {
                    RotateRight(current.Right!);
                    UpdateHeight(current.Right!);
                }

                var oldRoot = current;
                RotateLeft(oldRoot);

                UpdateHeight(oldRoot);
                if (oldRoot.Parent != null)
                {
                    UpdateHeight(oldRoot.Parent);
                    current = oldRoot.Parent.Parent;
                }
                else
                {
                    current = null;
                }
            }
            else
            {
                current = current.Parent;
            }
        }
    }

    private static int Height(AvlNode<TKey, TValue>? node) => node?.Height ?? 0;

    private static void UpdateHeight(AvlNode<TKey, TValue> node)
    {
        node.Height = Math.Max(Height(node.Left), Height(node.Right)) + 1;
    }

    private static int BalanceFactor(AvlNode<TKey, TValue> node)
    {
        return Height(node.Left) - Height(node.Right);
    }
}
