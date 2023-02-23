using System;
using Xunit;

namespace Dapperling.Tests;

#if DEBUG
public class IntegrationFactAttribute : FactAttribute
{
}
#else
public class IntegrationFactAttribute : Attribute
{
}
#endif
