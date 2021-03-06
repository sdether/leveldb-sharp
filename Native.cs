// leveldb-sharp
//
// Copyright (c) 2012 Mirco Bauer <meebey@meebey.net>
// Copyright (c) 2011 The LevelDB Authors
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//    * Redistributions of source code must retain the above copyright
//      notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above
//      copyright notice, this list of conditions and the following disclaimer
//      in the documentation and/or other materials provided with the
//      distribution.
//    * Neither the name of Google Inc. nor the names of its
//      contributors may be used to endorse or promote products derived from
//      this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace LevelDB
{
    public static class Native
    {
        static void CheckError(string error)
        {
            if (String.IsNullOrEmpty(error)) {
                return;
            }
            throw new ApplicationException(error);
        }

        static UIntPtr GetStringLength(string value)
        {
            return new UIntPtr((uint) Encoding.UTF8.GetByteCount(value));
        }

        // extern leveldb_t* leveldb_open(const leveldb_options_t* options, const char* name, char** errptr);
        [DllImport("leveldb")]
        public static extern IntPtr leveldb_open(IntPtr options, string name, out string error);
        public static IntPtr leveldb_open(IntPtr options, string name)
        {
            string error;
            var db = leveldb_open(options, name, out error);
            CheckError(error);
            return db;
        }

        // extern void leveldb_close(leveldb_t* db);
        [DllImport("leveldb")]
        public static extern void leveldb_close(IntPtr db);

        // extern void leveldb_put(leveldb_t* db, const leveldb_writeoptions_t* options, const char* key, size_t keylen, const char* val, size_t vallen, char** errptr);
        [DllImport("leveldb")]
        public static extern void leveldb_put(IntPtr db,
                                              IntPtr writeOptions,
                                              string key,
                                              UIntPtr keyLength,
                                              string value,
                                              UIntPtr valueLength,
                                              out string error);
        public static void leveldb_put(IntPtr db,
                                       IntPtr writeOptions,
                                       string key,
                                       string value)
        {
            string error;
            var keyLength = GetStringLength(key);
            var valueLength = GetStringLength(value);
            Native.leveldb_put(db, writeOptions,
                               key, keyLength,
                               value, valueLength, out error);
            CheckError(error);
        }

        // extern void leveldb_delete(leveldb_t* db, const leveldb_writeoptions_t* options, const char* key, size_t keylen, char** errptr);
        [DllImport("leveldb")]
        public static extern void leveldb_delete(IntPtr db, IntPtr writeOptions, string key, UIntPtr keylen, out string error);
        public static void leveldb_delete(IntPtr db, IntPtr writeOptions, string key)
        {
            string error;
            var keyLength = GetStringLength(key);
            leveldb_delete(db, writeOptions, key, keyLength, out error);
            CheckError(error);
        }

        // extern void leveldb_write(leveldb_t* db, const leveldb_writeoptions_t* options, leveldb_writebatch_t* batch, char** errptr);
        [DllImport("leveldb")]
        public static extern void leveldb_write(IntPtr db, IntPtr options, IntPtr batch, out string error);

        // extern char* leveldb_get(leveldb_t* db, const leveldb_readoptions_t* options, const char* key, size_t keylen, size_t* vallen, char** errptr);
        [DllImport("leveldb")]
        public static extern IntPtr leveldb_get(IntPtr db,
                                                IntPtr readOptions,
                                                string key,
                                                UIntPtr keyLength,
                                                out UIntPtr valueLength,
                                                out string error);
        public static string leveldb_get(IntPtr db,
                                         IntPtr readOptions,
                                         string key)
        {
            UIntPtr valueLength;
            string error;
            var keyLength = GetStringLength(key);
            var valuePtr = leveldb_get(db, readOptions, key, keyLength,
                                       out valueLength, out error);
            CheckError(error);
            if (valuePtr == IntPtr.Zero || valueLength == UIntPtr.Zero) {
                return null;
            }
            var value = Marshal.PtrToStringAnsi(valuePtr, (int) valueLength);
            return value;
        }

        // extern leveldb_iterator_t* leveldb_create_iterator(leveldb_t* db, const leveldb_readoptions_t* options);
        [DllImport("leveldb")]
        public static extern IntPtr leveldb_create_iterator(IntPtr db, IntPtr readOptions);

#region Options
        // extern leveldb_options_t* leveldb_options_create();
        [DllImport("leveldb")]
        public static extern IntPtr leveldb_options_create();

        // extern void leveldb_options_destroy(leveldb_options_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_options_destroy(IntPtr options);

        // extern void leveldb_options_set_comparator(leveldb_options_t*, leveldb_comparator_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_options_set_comparator(IntPtr options, IntPtr comparator);

        /// <summary>
        /// If true, the database will be created if it is missing.
        /// Default: false
        /// </summary>
        // extern void leveldb_options_set_create_if_missing(leveldb_options_t*, unsigned char);
        [DllImport("leveldb")]
        public static extern void leveldb_options_set_create_if_missing(IntPtr options, bool value);

        /// <summary>
        /// If true, an error is raised if the database already exists.
        /// Default: false
        /// </summary>
        // extern void leveldb_options_set_error_if_exists(leveldb_options_t*, unsigned char);
        [DllImport("leveldb")]
        public static extern void leveldb_options_set_error_if_exists(IntPtr options, bool value);

        /// <summary>
        /// If true, the implementation will do aggressive checking of the
        /// data it is processing and will stop early if it detects any
        /// errors.  This may have unforeseen ramifications: for example, a
        /// corruption of one DB entry may cause a large number of entries to
        /// become unreadable or for the entire DB to become unopenable.
        /// Default: false
        /// </summary>
        // extern void leveldb_options_set_paranoid_checks(leveldb_options_t*, unsigned char);
        [DllImport("leveldb")]
        public static extern void leveldb_options_set_paranoid_checks(IntPtr options, bool value);

        /// <summary>
        /// Number of open files that can be used by the DB.  You may need to
        /// increase this if your database has a large working set (budget
        /// one open file per 2MB of working set).
        ///
        /// Default: 1000
        /// </summary>
        // extern void leveldb_options_set_max_open_files(leveldb_options_t*, int);
        [DllImport("leveldb")]
        public static extern void leveldb_options_set_max_open_files(IntPtr options, int value);

        // extern void leveldb_options_set_compression(leveldb_options_t*, int);
        [DllImport("leveldb")]
        public static extern void leveldb_options_set_compression(IntPtr options, int value);

        /// <summary>
        /// Control over blocks (user data is stored in a set of blocks, and
        /// a block is the unit of reading from disk).
        ///
        /// If non-NULL, use the specified cache for blocks.
        /// If NULL, leveldb will automatically create and use an 8MB internal cache.
        /// Default: NULL
        /// </summary>
        // extern void leveldb_options_set_cache(leveldb_options_t*, leveldb_cache_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_options_set_cache(IntPtr options, IntPtr cache);
        public static void leveldb_options_set_cache_size(IntPtr options, int capacity)
        {
            var cache = leveldb_cache_create_lru((UIntPtr) capacity);
            leveldb_options_set_cache(options, cache);
        }

        /// <summary>
        /// Approximate size of user data packed per block.  Note that the
        /// block size specified here corresponds to uncompressed data.  The
        /// actual size of the unit read from disk may be smaller if
        /// compression is enabled.  This parameter can be changed dynamically.
        ///
        /// Default: 4K
        /// </summary>
        // extern void leveldb_options_set_block_size(leveldb_options_t*, size_t);

        /// <summary>
        /// Amount of data to build up in memory (backed by an unsorted log
        /// on disk) before converting to a sorted on-disk file.
        ///
        /// Larger values increase performance, especially during bulk loads.
        /// Up to two write buffers may be held in memory at the same time,
        /// so you may wish to adjust this parameter to control memory usage.
        /// Also, a larger write buffer will result in a longer recovery time
        /// the next time the database is opened.
        ///
        /// Default: 4MB
        /// </summary>
        // extern void leveldb_options_set_write_buffer_size(leveldb_options_t*, size_t);

        // TODO:
        /*
        extern void leveldb_options_set_write_buffer_size(leveldb_options_t*, size_t);
        extern void leveldb_options_set_cache(leveldb_options_t*, leveldb_cache_t*);
        extern void leveldb_options_set_block_size(leveldb_options_t*, size_t);
        extern void leveldb_options_set_block_restart_interval(leveldb_options_t*, int);
        enum {
          leveldb_no_compression = 0,
          leveldb_snappy_compression = 1
        };
        */
#endregion

#region Read Options
        // extern leveldb_readoptions_t* leveldb_readoptions_create();
        [DllImport("leveldb")]
        public static extern IntPtr leveldb_readoptions_create();

        // extern void leveldb_readoptions_destroy(leveldb_readoptions_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_readoptions_destroy(IntPtr readOptions);

        /*
        extern void leveldb_readoptions_set_verify_checksums(
            leveldb_readoptions_t*,
            unsigned char);
        extern void leveldb_readoptions_set_snapshot(
            leveldb_readoptions_t*,
            const leveldb_snapshot_t*);
        */

        // extern void leveldb_readoptions_set_fill_cache(leveldb_readoptions_t*, unsigned char);
        [DllImport("leveldb")]
        public static extern void leveldb_readoptions_set_fill_cache(IntPtr readOptions, bool value);
#endregion

#region Write Options
        // extern leveldb_writeoptions_t* leveldb_writeoptions_create();
        [DllImport("leveldb")]
        public static extern IntPtr leveldb_writeoptions_create();

        // extern void leveldb_writeoptions_destroy(leveldb_writeoptions_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_writeoptions_destroy(IntPtr writeOptions);

        /*
        extern void leveldb_writeoptions_set_sync(
            leveldb_writeoptions_t*, unsigned char);
        */
#endregion

#region Iterator
        // extern void leveldb_iter_seek_to_first(leveldb_iterator_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_iter_seek_to_first(IntPtr iter);

        // extern void leveldb_iter_seek_to_last(leveldb_iterator_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_iter_seek_to_last(IntPtr iter);

        // extern void leveldb_iter_seek(leveldb_iterator_t*, const char* k, size_t klen);
        [DllImport("leveldb")]
        public static extern void leveldb_iter_seek(IntPtr iter, string key, UIntPtr keyLength);
        public static void leveldb_iter_seek(IntPtr iter, string key)
        {
            var keyLength = GetStringLength(key);
            leveldb_iter_seek(iter, key, keyLength);
        }

        // extern unsigned char leveldb_iter_valid(const leveldb_iterator_t*);
        [DllImport("leveldb")]
        public static extern bool leveldb_iter_valid(IntPtr iter);

        // extern void leveldb_iter_prev(leveldb_iterator_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_iter_prev(IntPtr iter);

        // extern void leveldb_iter_next(leveldb_iterator_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_iter_next(IntPtr iter);

        // extern const char* leveldb_iter_key(const leveldb_iterator_t*, size_t* klen);
        [DllImport("leveldb")]
        public static extern IntPtr leveldb_iter_key(IntPtr iter, out UIntPtr keyLength);
        public static string leveldb_iter_key(IntPtr iter)
        {
            UIntPtr keyLength;
            var keyPtr = leveldb_iter_key(iter, out keyLength);
            if (keyPtr == IntPtr.Zero || keyLength == UIntPtr.Zero) {
                return null;
            }
            var key = Marshal.PtrToStringAnsi(keyPtr, (int) keyLength);
            return key;
        }

        // extern const char* leveldb_iter_value(const leveldb_iterator_t*, size_t* vlen);
        [DllImport("leveldb")]
        public static extern IntPtr leveldb_iter_value(IntPtr iter, out UIntPtr valueLength);
        public static string leveldb_iter_value(IntPtr iter)
        {
            UIntPtr valueLength;
            var valuePtr = leveldb_iter_value(iter, out valueLength);
            if (valuePtr == IntPtr.Zero || valueLength == UIntPtr.Zero) {
                return null;
            }
            var value = Marshal.PtrToStringAnsi(valuePtr, (int) valueLength);
            return value;
        }

        // extern void leveldb_iter_destroy(leveldb_iterator_t*);
        [DllImport("leveldb")]
        public static extern void leveldb_iter_destroy(IntPtr iter);

        /*
extern void leveldb_iter_get_error(const leveldb_iterator_t*, char** errptr);
         */
#endregion

#region Cache
        // extern leveldb_cache_t* leveldb_cache_create_lru(size_t capacity);
        [DllImport("leveldb")]
        public static extern IntPtr leveldb_cache_create_lru(UIntPtr capacity);

        // extern void leveldb_cache_destroy(leveldb_cache_t* cache);
        [DllImport("leveldb")]
        public static extern void leveldb_cache_destroy(IntPtr cache);
#endregion

        public static void Dump(IntPtr db)
        {
            var options = Native.leveldb_readoptions_create();
            IntPtr iter = Native.leveldb_create_iterator(db, options);
            for (Native.leveldb_iter_seek_to_first(iter);
                 Native.leveldb_iter_valid(iter);
                 Native.leveldb_iter_next(iter)) {
                string key = Native.leveldb_iter_key(iter);
                string value = Native.leveldb_iter_value(iter);
                Console.WriteLine("'{0}' => '{1}'", key, value);
            }
            Native.leveldb_iter_destroy(iter);
        }
    }
}
