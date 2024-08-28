namespace AF.Tutorial
{
    using System;
    using UnityEngine;

    public class TutorialManager : MonoBehaviour
    {
        public TutorialSection[] tutorialSections;

        TutorialSection activeTutorialSection;

        void Start()
        {
            activeTutorialSection = tutorialSections[0];
            activeTutorialSection.Activate();
        }

        public void Advance()
        {
            int idx = Array.IndexOf(tutorialSections, activeTutorialSection);

            if (idx + 1 <= tutorialSections.Length - 1)
            {
                activeTutorialSection = tutorialSections[idx + 1];
                activeTutorialSection.Activate();
            }
        }
        public void Return()
        {
            int idx = Array.IndexOf(tutorialSections, activeTutorialSection);

            if (idx - 1 >= 0)
            {
                activeTutorialSection = tutorialSections[idx - 1];
                activeTutorialSection.Activate();
            }
        }
    }
}