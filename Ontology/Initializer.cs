using RooTrax.Cache;

namespace Ontology
{
	public class Initializer
	{
		static public void Initialize()
		{
			CacheManager.UseAsyncLocal = true;
			ObjectCacheManager.UseAsyncLocal = true;

			Logs.DebugLog.MaxLinesPerFile = 0;
		}

		static public void ResetCache()
		{
			// Canonical global reset entrypoint.
			ResettablePrototypeAsyncLocal.ResetAll();
			TemporaryLexemes.Cache.Clear();
			TemporaryLexemes.m_mapRelatedParentPrototypes.Clear();
			TemporaryPrototypes.ResetCache();
		}
	}
}
