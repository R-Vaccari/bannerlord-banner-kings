using System;

namespace BannerKings.Managers.Religions
{
    public class Doctrine<T>
    {
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public Func<T, bool> CheckFollowsDoctrine { get; protected set; }

        protected Doctrine(string name, string description, Func<T, bool> doctrineFunc = null)
        {
            Name = name;
            Description = description;
            if (doctrineFunc != null)
                CheckFollowsDoctrine = doctrineFunc;
            else
                CheckFollowsDoctrine = t => true;
        }
    }

    // Test doctrine, commented out.
    // public class MaleOnly : Doctrine<Hero>
    // {
    //     protected MaleOnly() : base("name", "description", hero => !hero.IsFemale)
    //     {
    //         // To check if something follows doctrine
    //         CheckFollowsDoctrine(Hero.MainHero);
    //     }
    // }
}