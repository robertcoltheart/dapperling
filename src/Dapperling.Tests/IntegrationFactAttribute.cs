using System;
using Xunit;

namespace Dapper.Tests;

#if DEBUG
public class IntegrationFactAttribute : FactAttribute
{
}
#else
public class IntegrationFactAttribute : Attribute
{
}
#endif
