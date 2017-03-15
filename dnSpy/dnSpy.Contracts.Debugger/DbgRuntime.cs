﻿/*
    Copyright (C) 2014-2017 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;

namespace dnSpy.Contracts.Debugger {
	/// <summary>
	/// A runtime in a process
	/// </summary>
	public abstract class DbgRuntime : DbgObject {
		/// <summary>
		/// Gets the process
		/// </summary>
		public abstract DbgProcess Process { get; }

		/// <summary>
		/// Gets the runtime name
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Gets all app domains
		/// </summary>
		public abstract DbgAppDomain[] AppDomains { get; }

		/// <summary>
		/// Raised when <see cref="AppDomains"/> is changed
		/// </summary>
		public abstract event EventHandler<DbgCollectionChangedEventArgs<DbgAppDomain>> AppDomainsChanged;

		/// <summary>
		/// Gets all modules
		/// </summary>
		public abstract DbgModule[] Modules { get; }

		/// <summary>
		/// Raised when <see cref="Modules"/> is changed
		/// </summary>
		public abstract event EventHandler<DbgCollectionChangedEventArgs<DbgModule>> ModulesChanged;

		/// <summary>
		/// Gets all threads
		/// </summary>
		public abstract DbgThread[] Threads { get; }

		/// <summary>
		/// Raised when <see cref="Threads"/> is changed
		/// </summary>
		public abstract event EventHandler<DbgCollectionChangedEventArgs<DbgThread>> ThreadsChanged;
	}
}
