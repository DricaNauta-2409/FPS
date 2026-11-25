using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Ferramenta de uso unico: reduz o mapa inteiro para que o player fique com escala 1,1,1.
// Menu: Tools > Reescalar mundo para o Player (1,1,1)
//
// Como funciona: o fator e a escala atual do player (ex.: 10). Cada objeto RAIZ da cena tem a
// posicao e a escala divididas por esse fator, em torno da origem. Como os filhos sao relativos
// aos pais, o mapa inteiro encolhe junto e o layout fica identico, so que na escala certa.
// Valores em metros "do mundo" (alcance de luzes, alcance do inimigo, velocidades) tambem sao
// divididos, para o jogo continuar se comportando igual. Tudo pode ser desfeito com Ctrl+Z.
public static class RescaleWorldToPlayer
{
    [MenuItem("Tools/Reescalar mundo para o Player (1,1,1)")]
    private static void Run()
    {
        Scene scene = SceneManager.GetActiveScene();
        PlayerMovement player = Object.FindObjectOfType<PlayerMovement>(true);
        if (player == null)
        {
            EditorUtility.DisplayDialog("Reescalar mundo", "Nao achei nenhum objeto com PlayerMovement na cena.", "OK");
            return;
        }

        Transform playerT = player.transform;
        if (playerT.parent != null)
        {
            EditorUtility.DisplayDialog("Reescalar mundo", "O player precisa estar na raiz da hierarquia.", "OK");
            return;
        }

        float k = playerT.localScale.x;
        if (Mathf.Approximately(k, 1f))
        {
            EditorUtility.DisplayDialog("Reescalar mundo", "O player ja esta com escala 1. Nada a fazer.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Reescalar mundo",
                $"O player esta com escala {playerT.localScale}.\n\n" +
                $"Todo o mapa da cena '{scene.name}' vai ser dividido por {k} para o player ficar 1,1,1.\n" +
                "Da para desfazer com Ctrl+Z.", "Reescalar", "Cancelar"))
            return;

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Reescalar mundo para o Player");

        var log = new List<string>();
        scaledPrefabs = new HashSet<string>();
        int roots = 0, lights = 0, audios = 0, agents = 0, particles = 0, probes = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            // UI e EventSystem nao fazem parte do mundo 3D.
            if (root.GetComponent<RectTransform>() != null || root.GetComponent<EventSystem>() != null)
                continue;

            Transform t = root.transform;
            Undo.RecordObject(t, "Reescalar");
            t.localPosition /= k;
            t.localScale = t == playerT ? Vector3.one : t.localScale / k;
            roots++;

            foreach (Light l in root.GetComponentsInChildren<Light>(true))
            {
                if (l.type == LightType.Directional) continue;
                Undo.RecordObject(l, "Reescalar");
                l.range /= k;
                lights++;
            }

            foreach (AudioSource a in root.GetComponentsInChildren<AudioSource>(true))
            {
                Undo.RecordObject(a, "Reescalar");
                a.minDistance /= k;
                a.maxDistance /= k;
                audios++;
            }

            foreach (NavMeshAgent ag in root.GetComponentsInChildren<NavMeshAgent>(true))
            {
                Undo.RecordObject(ag, "Reescalar");
                ag.speed /= k;
                ag.acceleration /= k;
                ag.stoppingDistance /= k;
                agents++;
            }

            foreach (Enemy e in root.GetComponentsInChildren<Enemy>(true))
                DivideFloats(e, k, "chaseRange", "attackRange");

            particles += root.GetComponentsInChildren<ParticleSystem>(true).Length;
            probes += root.GetComponentsInChildren<ReflectionProbe>(true).Length;
        }

        // Velocidade e pulo do player estavam em unidades do mundo antigo (10x maior).
        // A gravidade fica -9.81, que agora e a gravidade real em metros.
        DivideFloats(player, k, "moveSpeed", "jumpleHeight");

        // A bala do inimigo e um prefab separado da cena; reduz a escala dela tambem.
        foreach (Enemy e in Object.FindObjectsOfType<Enemy>(true))
        {
            var so = new SerializedObject(e);
            var bullet = so.FindProperty("Bullet").objectReferenceValue as GameObject;
            if (bullet != null && ScaleBulletPrefab(bullet, k))
                log.Add($"Prefab '{bullet.name}' reduzido.");
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);

        log.Insert(0, $"{roots} objetos raiz reduzidos por {k}. Luzes: {lights}, AudioSources: {audios}, NavMeshAgents: {agents}.");
        if (particles > 0) log.Add($"Atencao: {particles} ParticleSystem(s) na cena, confira se o tamanho ficou bom.");
        if (probes > 0) log.Add($"Atencao: {probes} ReflectionProbe(s) na cena, confira o Box Size.");
        log.Add("Agora salve a cena (Ctrl+S) e faca o Bake do Navmesh de novo (objeto 'Navmesh' > Bake).");

        Debug.Log("[Reescalar mundo]\n" + string.Join("\n", log));
        EditorUtility.DisplayDialog("Reescalar mundo", string.Join("\n\n", log), "OK");
    }

    private static void DivideFloats(Object target, float k, params string[] names)
    {
        var so = new SerializedObject(target);
        foreach (string n in names)
        {
            SerializedProperty p = so.FindProperty(n);
            if (p != null && p.propertyType == SerializedPropertyType.Float)
                p.floatValue /= k;
        }
        so.ApplyModifiedProperties(); // registra Undo e override de prefab automaticamente
    }

    private static HashSet<string> scaledPrefabs;

    private static bool ScaleBulletPrefab(GameObject prefabAsset, float k)
    {
        string path = AssetDatabase.GetAssetPath(prefabAsset);
        if (string.IsNullOrEmpty(path) || !scaledPrefabs.Add(path)) return false;

        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        contents.transform.localScale /= k;
        PrefabUtility.SaveAsPrefabAsset(contents, path);
        PrefabUtility.UnloadPrefabContents(contents);
        return true;
    }
}
