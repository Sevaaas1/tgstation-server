﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tgstation.Server.Api.Models;
using Tgstation.Server.Api.Rights;

namespace Tgstation.Server.Client.Components
{
	/// <summary>
	/// <see cref="IServerClient"/> for managing <see cref="Token"/>s
	/// </summary>
	public interface ITokenClient: IClient<TokenRights>
	{
		/// <summary>
		/// Generates a new token for the <see cref="IServerClient"/>'s <see cref="System.Net.IPAddress"/>
		/// </summary>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation</param>
		/// <returns>A <see cref="Task"/> resulting in a new <see cref="Token"/> for the <see cref="IServerClient"/></returns>
		Task<Token> Generate(CancellationToken cancellationToken);

		/// <summary>
		/// Gets all active <see cref="TokenInfo"/>s for the <see cref="IServerClient"/>
		/// </summary>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation</param>
		/// <returns>A <see cref="Task"/> resulting a <see cref="IReadOnlyList{T}"/> of active <see cref="TokenInfo"/>s for the <see cref="IServerClient"/></returns>
		Task<IReadOnlyList<TokenInfo>> GetClientInfos(CancellationToken cancellationToken);

		/// <summary>
		/// Gets all active <see cref="TokenInfo"/>s for all users
		/// </summary>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation</param>
		/// <returns>A <see cref="Task"/> resulting a <see cref="IReadOnlyDictionary{TKey, TValue}"/> of active <see cref="TokenInfo"/>s for the <see cref="IServerClient"/> keyed by username</returns>
		Task<IReadOnlyDictionary<string, TokenInfo>> GetAllInfos(CancellationToken cancellationToken);

		/// <summary>
		/// Invalidate the specified active <see cref="Token"/> designated by <paramref name="tokenId"/>
		/// </summary>
		/// <param name="tokenId">The <see cref="TokenInfo.Id"/> of the <see cref="Token"/> to invalidate</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation</param>
		/// <returns>A <see cref="Task"/> representing the running operation</returns>
		Task Invalidate(long tokenId, CancellationToken cancellationToken);

		/// <summary>
		/// Invalidate all active <see cref="Token"/>s for a <see cref="IServerClient"/>
		/// </summary>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation</param>
		/// <returns>A <see cref="Task"/> representing the running operation</returns>
		Task InvalidateAllClient(CancellationToken cancellationToken);

		/// <summary>
		/// Invalidate all active <see cref="Token"/>s
		/// </summary>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation</param>
		/// <returns>A <see cref="Task"/> representing the running operation</returns>
		Task InvalidateAll(CancellationToken cancellationToken);
	}
}
