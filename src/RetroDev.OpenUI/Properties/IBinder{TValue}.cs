namespace RetroDev.OpenUI.Properties;

// TODO: other than property binders we can have other types of binders?
// e.g. DB binder where NotifySourceChanged() is storing to database

internal interface IBinder
{
    void Unbind();
}
