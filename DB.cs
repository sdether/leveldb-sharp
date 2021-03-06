// leveldb-sharp
//
//  Copyright (c) 2012, Mirco Bauer <meebey@meebey.net>
//  All rights reserved.
// 
//  Redistribution and use in source and binary forms, with or without
//  modification, are permitted provided that the following conditions are
//  met:
// 
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
//       copyright notice, this list of conditions and the following disclaimer
//       in the documentation and/or other materials provided with the
//       distribution.
//     * Neither the name of Google Inc. nor the names of its
//       contributors may be used to endorse or promote products derived from
//       this software without specific prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
//  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
//  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
//  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
//  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections;
using System.Collections.Generic;

namespace LevelDB
{
    public class DB : IDisposable, IEnumerable<KeyValuePair<string, string>>
    {
        public IntPtr Handle { get; private set; }
        Options Options { get; set; }
        bool Disposed { get; set; }

        public string this[string key] {
            get {
                return Get(null, key);
            }
            set {
                Put(null, key, value);
            }
        }

        public DB(Options options, string path)
        {
            if (options == null) {
                options = new Options();
            }
            // keep a reference to options as it might contain a cache object
            // which needs to stay alive as long as the DB is not closed
            Options = options;
            Handle = Native.leveldb_open(options.Handle, path);
        }

        ~DB()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            var disposed = Disposed;
            if (disposed) {
                return;
            }
            Disposed = true;

            if (disposing) {
                // free managed
                Options = null;
            }
            // free unmanaged
            var handle = Handle;
            if (handle != IntPtr.Zero) {
                Handle = IntPtr.Zero;
                Native.leveldb_close(handle);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static DB Open(Options options, string path)
        {
            return new DB(options, path);
        }

        public void Put(WriteOptions options, string key, string value)
        {
            CheckDisposed();
            if (options == null) {
                options = new WriteOptions();
            }
            Native.leveldb_put(Handle, options.Handle, key, value);
        }

        public void Put(string key, string value)
        {
            Put(null, key, value);
        }

        public void Delete(WriteOptions options, string key)
        {
            CheckDisposed();
            if (options == null) {
                options = new WriteOptions();
            }
            Native.leveldb_delete(Handle, options.Handle, key);
        }

        public void Delete(string key)
        {
            Delete(null, key);
        }

        public string Get(ReadOptions options, string key)
        {
            CheckDisposed();
            if (options == null) {
                options = new ReadOptions();
            }
            return Native.leveldb_get(Handle, options.Handle, key);
        }

        public string Get(string key)
        {
            return Get(null, key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            CheckDisposed();
            var options = new ReadOptions();
            IntPtr iter = IntPtr.Zero;
            try {
                iter = Native.leveldb_create_iterator(Handle, options.Handle);
                for (Native.leveldb_iter_seek_to_first(iter);
                     Native.leveldb_iter_valid(iter);
                     Native.leveldb_iter_next(iter)) {
                    string key = Native.leveldb_iter_key(iter);
                    string value = Native.leveldb_iter_value(iter);
                    var kvp = new KeyValuePair<string, string>(key, value);
                    yield return kvp;
                }
            } finally {
                if (iter != IntPtr.Zero) {
                    Native.leveldb_iter_destroy(iter);
                }
            }
        }

        void CheckDisposed()
        {
            if (!Disposed) {
                return;
            }
            throw new ObjectDisposedException(this.GetType().Name);
        }
    }
}
