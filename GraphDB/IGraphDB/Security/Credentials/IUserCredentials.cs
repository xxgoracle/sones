﻿using System;

namespace sones.GraphDB.Security
{
    /// <summary>
    /// Used for authentication
    /// 
    /// there might be implementations for some kind of base authentication or via public key cryptography
    /// </summary>
    public interface IUserCredentials
    {
        /// <summary>
        /// Get the login
        /// </summary>
        String Login { get; }
    }
}