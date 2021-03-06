﻿using System;
using System.Text;

namespace Enkoni.Framework.Serialization {
  /// <summary>Represents a transformer that transforms an instance of <typeparamref name="T"/> into a byte array or string
  /// and vice versa.</summary>
  /// <typeparam name="T">The type that must be transformed.</typeparam>
  public abstract class Transformer<T> {
    #region Constructor

    /// <summary>Initializes a new instance of the <see cref="Transformer{T}"/> class.</summary>
    protected Transformer() {
    }

    #endregion

    #region Public methods

    /// <summary>Transforms <paramref name="instance"/> into a string.</summary>
    /// <param name="instance">The instance that must be transformed.</param>
    /// <returns>The string that contains the transformed instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <see langword="null"/>.</exception>
    public string ToString(T instance) {
      Guard.ArgumentIsNotNull(instance, nameof(instance));

      return this.ToStringCore(instance);
    }

    /// <summary>Transforms <paramref name="instance"/> into a byte array.</summary>
    /// <param name="instance">The instance that must be transformed.</param>
    /// <param name="encoding">The encoding that must be used to transform the instance into bytes.</param>
    /// <returns>The byte array that contains the transformed instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
    public byte[] ToBytes(T instance, Encoding encoding) {
      Guard.ArgumentIsNotNull(instance, nameof(instance));
      Guard.ArgumentIsNotNull(encoding, nameof(encoding));

      return this.ToBytesCore(instance, encoding);
    }

    /// <summary>Transforms <paramref name="instance"/> into a byte array.</summary>
    /// <param name="instance">The instance that must be transformed.</param>
    /// <param name="encoding">The encoding that must be used to transform the instance into bytes.</param>
    /// <param name="bytes">The byte array into which the transformed instance must be stored.</param>
    /// <param name="offset">The offset in <paramref name="bytes"/>.</param>
    /// <returns>The number of bytes that were written into <paramref name="bytes"/>.</returns>
    /// <exception cref="ArgumentException">The byte array has a zero length.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The offset is outside the bounds of the byte array.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="instance"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
    public int ToBytes(T instance, Encoding encoding, byte[] bytes, int offset) {
      Guard.ArgumentIsNotNull(instance, nameof(instance));
      Guard.ArgumentIsNotNull(encoding, nameof(encoding));
      Guard.ArgumentIsNotNull(bytes, nameof(bytes));
      Guard.ArgumentIsGreaterOrEqualThan(0, offset, nameof(offset), "The offset cannot be negative");
      Guard.ArgumentIsLowerThan(bytes.Length, offset, nameof(offset), "The offset must be within the bounds of the array");

      if (bytes.Length == 0) {
        throw new ArgumentException("Cannot transform an instance of type " + typeof(T) + " into an empty array", "bytes");
      }

      return this.ToBytesCore(instance, encoding, bytes, offset);
    }

    /// <summary>Transforms a string value into an instance of type <typeparamref name="T"/>.</summary>
    /// <param name="input">The string that must be transformed.</param>
    /// <returns>The transformed instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public T FromString(string input) {
      Guard.ArgumentIsNotNull(input, nameof(input));

      return this.FromStringCore(input);
    }

    /// <summary>Transforms the content of a byte array into an instance of type <typeparamref name="T"/>.</summary>
    /// <param name="bytes">The byte array whose content must be transformed.</param>
    /// <param name="encoding">The encoding that must be used to transform the bytes.</param>
    /// <returns>The transformed instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The byte array has a length of zero.</exception>
    public T FromBytes(byte[] bytes, Encoding encoding) {
      Guard.ArgumentIsNotNull(bytes, nameof(bytes));
      Guard.ArgumentIsNotNull(encoding, nameof(encoding));

      if(bytes.Length == 0) {
        throw new ArgumentException("Cannot transform an instance of type " + typeof(T) + " into an empty array", "bytes");
      }

      return this.FromBytesCore(bytes, encoding);
    }

    /// <summary>Transforms the content of a byte array into an instance of type <typeparamref name="T"/>.</summary>
    /// <param name="bytes">The byte array whose content must be transformed.</param>
    /// <param name="offset">The offset from which to start reading bytes.</param>
    /// <param name="length">The number of bytes that must be read from the array.</param>
    /// <param name="encoding">The encoding that must be used to transform the bytes.</param>
    /// <returns>The transformed instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The specified length is less then or equal to zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The specified offset is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The specified offset + length is outside the bounds of the array.
    /// </exception>
    /// <exception cref="ArgumentException">The byte array has a length of zero.</exception>
    public T FromBytes(byte[] bytes, int offset, int length, Encoding encoding) {
      Guard.ArgumentIsNotNull(bytes, nameof(bytes));
      Guard.ArgumentIsNotNull(encoding, nameof(encoding));
      Guard.ArgumentIsGreaterOrEqualThan(0, offset, nameof(offset), "The offset cannot be negative.");
      Guard.ArgumentIsGreaterThan(0, length, nameof(length), "The offset must be greater then zero.");

      if (bytes.Length == 0) {
        throw new ArgumentException("The byte array cannot be empty", "bytes");
      }

      if(offset + length >= bytes.Length) {
        throw new ArgumentOutOfRangeException("offset", offset, "The offset + length is outside the bounds of the array.");
      }

      return this.FromBytesCore(bytes, offset, length, encoding);
    }
    #endregion

    #region Protected extension methods

    /// <summary>Transforms <paramref name="instance"/> into a string.</summary>
    /// <param name="instance">The instance that must be transformed.</param>
    /// <returns>The string that contains the transformed instance.</returns>
    protected abstract string ToStringCore(T instance);

    /// <summary>Transforms <paramref name="instance"/> into a byte array.</summary>
    /// <param name="instance">The instance that must be transformed.</param>
    /// <param name="encoding">The encoding that must be used to transform the instance into bytes.</param>
    /// <returns>The byte array that contains the transformed instance.</returns>
    protected abstract byte[] ToBytesCore(T instance, Encoding encoding);

    /// <summary>Transforms <paramref name="instance"/> into a byte array.</summary>
    /// <param name="instance">The instance that must be transformed.</param>
    /// <param name="encoding">The encoding that must be used to transform the instance into bytes.</param>
    /// <param name="bytes">The byte array into which the transformed instance must be stored.</param>
    /// <param name="offset">The offset in <paramref name="bytes"/>.</param>
    /// <returns>The number of bytes that were written into <paramref name="bytes"/>.</returns>
    protected abstract int ToBytesCore(T instance, Encoding encoding, byte[] bytes, int offset);

    /// <summary>Transforms a string value into an instance of type <typeparamref name="T"/>.</summary>
    /// <param name="input">The string that must be transformed.</param>
    /// <returns>The transformed instance.</returns>
    protected abstract T FromStringCore(string input);

    /// <summary>Transforms the content of a byte array into an instance of type <typeparamref name="T"/>.</summary>
    /// <param name="bytes">The byte array whose content must be transformed.</param>
    /// <param name="offset">The offset from which to start reading bytes.</param>
    /// <param name="length">The number of bytes that must be read from the array.</param>
    /// <param name="encoding">The encoding that must be used to transform the bytes.</param>
    /// <returns>The transformed instance.</returns>
    protected abstract T FromBytesCore(byte[] bytes, int offset, int length, Encoding encoding);

    /// <summary>Transforms the content of a byte array into an instance of type <typeparamref name="T"/>.</summary>
    /// <param name="bytes">The byte array whose content must be transformed.</param>
    /// <param name="encoding">The encoding that must be used to transform the bytes.</param>
    /// <returns>The transformed instance.</returns>
    protected abstract T FromBytesCore(byte[] bytes, Encoding encoding);

    #endregion
  }
}
