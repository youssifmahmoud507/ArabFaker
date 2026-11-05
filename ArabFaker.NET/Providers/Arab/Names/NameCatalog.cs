namespace ArabFaker.NET.Providers.Arab.Names
{
    internal static class NameCatalog
    {
        internal static readonly NameEntry[] MaleFirstNames = [new("محمد", "Mohamed"), new("أحمد", "Ahmed"), new("محمود", "Mahmoud"), new("يوسف", "Youssef"), new("عمر", "Omar"), new("علي", "Ali"), new("إبراهيم", "Ibrahim"), new("خالد", "Khaled"), new("حسن", "Hassan"), new("حسين", "Hussein"), new("كريم", "Karim"), new("مصطفى", "Mostafa"), new("عمرو", "Amr"), new("طارق", "Tarek"), new("ياسين", "Yassin")];

        internal static readonly NameEntry[] FemaleFirstNames = [new("مريم", "Mariam"), new("فاطمة", "Fatma"), new("سارة", "Sara"), new("نور", "Nour"), new("ملك", "Malak"), new("آية", "Aya"), new("سلمى", "Salma"), new("فرح", "Farah"), new("ياسمين", "Yasmin"), new("ندى", "Nada"), new("حبيبة", "Habiba"), new("منة", "Menna"), new("رنا", "Rana"), new("ليلى", "Laila"), new("جنى", "Jana")];

        internal static readonly NameEntry[] FamilyNames = [new("محمد", "Mohamed"), new("أحمد", "Ahmed"), new("محمود", "Mahmoud"), new("إبراهيم", "Ibrahim"), new("علي", "Ali"), new("حسن", "Hassan"), new("حسين", "Hussein"), new("السيد", "Elsayed"), new("عبدالله", "Abdullah"), new("سالم", "Salem"), new("مصطفى", "Mostafa"), new("منصور", "Mansour"), new("عمر", "Omar"), new("كمال", "Kamal"), new("عبدالرحمن", "Abdelrahman")];

        internal static readonly NameEntry[] AllFirstNames =[.. MaleFirstNames, .. FemaleFirstNames];
    }

}
