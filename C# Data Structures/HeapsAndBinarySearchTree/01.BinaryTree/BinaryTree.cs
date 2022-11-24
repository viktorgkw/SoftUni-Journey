﻿namespace _01.BinaryTree
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class BinaryTree<T> : IAbstractBinaryTree<T>
    {
        public BinaryTree(T element, IAbstractBinaryTree<T> left, IAbstractBinaryTree<T> right)
        {
            Value = element;
            LeftChild = left;
            RightChild = right;
        }

        public T Value { get; private set; }

        public IAbstractBinaryTree<T> LeftChild { get; private set; }

        public IAbstractBinaryTree<T> RightChild { get; private set; }

        public string AsIndentedPreOrder(int indent)
        {
            StringBuilder sb = new StringBuilder();

            PreOrderDfs(sb, indent, this);

            return sb.ToString();
        }

        private void PreOrderDfs(StringBuilder sb, int indent, IAbstractBinaryTree<T> binaryTree)
        {
            sb.Append(new string(' ', indent))
                .AppendLine(binaryTree.Value.ToString());

            if (binaryTree.LeftChild != null)
            {
                PreOrderDfs(sb, indent + 2, binaryTree.LeftChild);
            }
            if (binaryTree.RightChild != null)
            {
                PreOrderDfs(sb, indent + 2, binaryTree.RightChild);
            }
        }

        public void ForEachInOrder(Action<T> action)
        {
            if (LeftChild != null)
            {
                LeftChild.ForEachInOrder(action);
            }
            action.Invoke(this.Value);
            if (RightChild != null)
            {
                RightChild.ForEachInOrder(action);
            }
        }

        public IEnumerable<IAbstractBinaryTree<T>> InOrder()
        {
            // Left => Right => Root
            var result = new List<IAbstractBinaryTree<T>>();

            if (LeftChild != null)
            {
                result.AddRange(LeftChild.InOrder());
            }

            result.Add(this);

            if (RightChild != null)
            {
                result.AddRange(RightChild.InOrder());
            }

            return result;
        }

        public IEnumerable<IAbstractBinaryTree<T>> PostOrder()
        {
            // Left => Root => Right
            var result = new List<IAbstractBinaryTree<T>>();

            if (LeftChild != null)
            {
                result.AddRange(LeftChild.PostOrder());
            }
            if (RightChild != null)
            {
                result.AddRange(RightChild.PostOrder());
            }

            result.Add(this);

            return result;
        }

        public IEnumerable<IAbstractBinaryTree<T>> PreOrder()
        {
            // Root => Left => Right
            var result = new List<IAbstractBinaryTree<T>>();

            result.Add(this);

            if (LeftChild != null)
            {
                result.AddRange(LeftChild.PreOrder());
            }
            if (RightChild != null)
            {
                result.AddRange(RightChild.PreOrder());
            }            

            return result;
        }
    }
}
