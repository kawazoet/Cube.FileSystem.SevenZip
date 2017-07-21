﻿/* ------------------------------------------------------------------------- */
///
/// Copyright (c) 2010 CubeSoft, Inc.
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Lesser General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Lesser General Public License for more details.
///
/// You should have received a copy of the GNU Lesser General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.
///
/* ------------------------------------------------------------------------- */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace Cube.FileSystem.SevenZip
{
    /* --------------------------------------------------------------------- */
    ///
    /// ArchiveOptionSetter
    /// 
    /// <summary>
    /// 圧縮ファイルのオプション項目を設定するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    internal class ArchiveOptionSetter
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// ArchiveOptionSetter
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="option">オプション</param>
        ///
        /* ----------------------------------------------------------------- */
        public ArchiveOptionSetter(ArchiveOption option)
        {
            Option = option;
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Option
        ///
        /// <summary>
        /// オプション内容を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public ArchiveOption Option { get; }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Execute
        ///
        /// <summary>
        /// オプションをアーカイブ・オブジェクトに設定します。
        /// </summary>
        /// 
        /// <param name="dest">アーカイブ・オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public virtual void Execute(ISetProperties dest)
        {
            if (Option == null || dest == null) return;

            var sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            sp.Demand();

            var cl = PropVariant.Create((uint)Option.CompressionLevel);
            var kh = Create(new[] { ToBstr("x") }.Concat(_dic.Keys.Select(o => ToBstr(o))).ToArray());
            var vh = Create(new[] { cl }.Concat(_dic.Values).ToArray());

            try
            {
                var n = _dic.Count + 1;
                dest.SetProperties(kh.AddrOfPinnedObject(), vh.AddrOfPinnedObject(), n);
            }
            finally
            {
                kh.Free();
                vh.Free();
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Add
        ///
        /// <summary>
        /// オプションを追加します。
        /// </summary>
        /// 
        /// <param name="name">名前</param>
        /// <param name="value">値</param>
        ///
        /* ----------------------------------------------------------------- */
        protected void Add(string name, bool value) => Add(name, PropVariant.Create(value));
        protected void Add(string name, uint value) => Add(name, PropVariant.Create(value));
        protected void Add(string name, ulong value) => Add(name, PropVariant.Create(value));
        protected void Add(string name, string value) => Add(name, PropVariant.Create(value));
        protected void Add(string name, DateTime value) => Add(name, PropVariant.Create(value));
        protected void Add(string name, PropVariant value) => _dic.Add(name, value);

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// ToBstr
        ///
        /// <summary>
        /// ネイティブな文字列型に変換します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private IntPtr ToBstr(string value) => Marshal.StringToBSTR(value);

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// アンマネージなオブジェクトを生成します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private GCHandle Create(object obj) => GCHandle.Alloc(obj, GCHandleType.Pinned);

        #endregion

        #region Fields
        private IDictionary<string, PropVariant> _dic = new Dictionary<string, PropVariant>();
        #endregion
    }
}