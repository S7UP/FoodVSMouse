// ����������Դ�����Ľӿڣ�ÿ�ֹ�����ȡ����Դ����ͬ�����������÷��ͽӿ�
public interface IBaseResourceFactory<T>
{
    T GetSingleResources(string resourcePath);
    void UnLoad(string resourcePath);
}
